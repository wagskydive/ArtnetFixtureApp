using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

public class LocalWebUiServer : MonoBehaviour
{
    public event Action OnServerStarted;

    private class MainThreadInvocation
    {
        public Func<string> Action;
        public ManualResetEventSlim WaitHandle;
        public string Result;
        public Exception Exception;
    }

    [SerializeField] private WebUiSettingsBridge settingsBridge;
    [SerializeField] private int port = 8080;
    [SerializeField] private string webUiFileName = "index.html";
    [SerializeField] private string persistentWebUiFolderName = "WebUi";

    private readonly Queue<MainThreadInvocation> _mainThreadQueue = new Queue<MainThreadInvocation>(8);
    private readonly object _queueLock = new object();

    private HttpListener _listener;
    private Thread _serverThread;
    private volatile bool _isRunning;
    private byte[] _cachedHtmlBytes;
    private string _persistentWebUiFilePath;

    public int Port => port;

    private void Awake()
    {
        CopyWebUiFileToPersistentPath();
        CacheHtmlPayload();
    }

    private void Start()
    {
        if (!HttpListener.IsSupported)
        {
            Debug.LogWarning("HttpListener is not supported on this platform.");
            return;
        }

        StartServer();
    }

    private void Update()
    {
        ProcessMainThreadQueue();
    }

    private void OnDestroy()
    {
        StopServer();
    }

    private void CacheHtmlPayload()
    {
        string html = LoadHtmlFromPersistentCopy();
        _cachedHtmlBytes = Encoding.UTF8.GetBytes(html);
    }

    public byte[] GetCachedHtmlBytes()
    {
        return _cachedHtmlBytes;
    }

    public string GetPersistentWebUiFilePath()
    {
        return _persistentWebUiFilePath;
    }

    private void CopyWebUiFileToPersistentPath()
    {
        try
        {
            string fileName = string.IsNullOrWhiteSpace(webUiFileName) ? "index.html" : webUiFileName;
            string persistentDirectory = Path.Combine(Application.persistentDataPath, persistentWebUiFolderName);
            Directory.CreateDirectory(persistentDirectory);

            _persistentWebUiFilePath = Path.Combine(persistentDirectory, fileName);

            byte[] htmlBytes = ReadStreamingAssetBytes(fileName);
            if (htmlBytes == null || htmlBytes.Length == 0)
            {
                return;
            }

            File.WriteAllBytes(_persistentWebUiFilePath, htmlBytes);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"LocalWebUiServer failed to copy WebUI HTML to persistent storage: {ex.Message}");
        }
    }

    private string LoadHtmlFromPersistentCopy()
    {
        string fallbackHtml = "<html><body>Missing StreamingAssets/index.html content.</body></html>";

        try
        {
            if (!string.IsNullOrEmpty(_persistentWebUiFilePath) && File.Exists(_persistentWebUiFilePath))
            {
                return File.ReadAllText(_persistentWebUiFilePath, Encoding.UTF8);
            }

            byte[] bytes = ReadStreamingAssetBytes(webUiFileName);
            if (bytes != null && bytes.Length > 0)
            {
                return Encoding.UTF8.GetString(bytes);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"LocalWebUiServer failed to load persistent WebUI HTML: {ex.Message}");
        }

        return fallbackHtml;
    }

    private byte[] ReadStreamingAssetBytes(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        try
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject assetManager = activity.Call<AndroidJavaObject>("getAssets");

                using (var inputStream = assetManager.Call<AndroidJavaObject>("open", fileName))
                using (var outputStream = new AndroidJavaObject("java.io.ByteArrayOutputStream"))
                {
                    var buffer = new byte[4096];
                    while (true)
                    {
                        int bytesRead = inputStream.Call<int>("read", buffer);
                        if (bytesRead <= 0)
                        {
                            break;
                        }

                        outputStream.Call("write", buffer, 0, bytesRead);
                    }

                    return outputStream.Call<byte[]>("toByteArray");
                }
            }
#else
            string htmlPath = Path.Combine(Application.streamingAssetsPath, fileName);
            if (File.Exists(htmlPath))
            {
                return File.ReadAllBytes(htmlPath);
            }
#endif
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"LocalWebUiServer failed to read StreamingAssets file '{fileName}': {ex.Message}");
        }

        return null;
    }

    private void StartServer()
    {
        if (_isRunning)
        {
            return;
        }

        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://+:{port}/");

        try
        {
            _listener.Start();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"LocalWebUiServer failed to start on port {port}: {ex.Message}");
            _listener.Close();
            _listener = null;
            return;
        }

        _isRunning = true;
        _serverThread = new Thread(ServerLoop)
        {
            IsBackground = true,
            Name = "LocalWebUiServer"
        };
        _serverThread.Start();

        Debug.Log($"Local web UI server listening on port {port}");
        OnServerStarted?.Invoke();
    }

    private void StopServer()
    {
        _isRunning = false;

        if (_listener != null)
        {
            try
            {
                _listener.Stop();
                _listener.Close();
            }
            catch (Exception)
            {
                // Keep shutdown resilient.
            }

            _listener = null;
        }

        if (_serverThread != null && _serverThread.IsAlive)
        {
            _serverThread.Join(300);
            _serverThread = null;
        }
    }

    private void ServerLoop()
    {
        while (_isRunning && _listener != null)
        {
            HttpListenerContext context = null;
            try
            {
                context = _listener.GetContext();
                HandleContext(context);
            }
            catch (Exception ex)
            {
                if (_isRunning)
                {
                    Debug.LogWarning($"LocalWebUiServer request loop hit an exception: {ex.Message}");
                }

                if (context != null)
                {
                    SafeClose(context.Response);
                }
            }
        }
    }

    private void HandleContext(HttpListenerContext context)
    {
        if (context == null || context.Request == null || context.Response == null)
        {
            return;
        }

        string path = context.Request.Url != null ? context.Request.Url.AbsolutePath : "/";
        if (path == "/" || path == "/index.html")
        {
            WriteHtml(context.Response);
            return;
        }

        if (TryWriteStaticFile(path, context.Response))
        {
            return;
        }

        if (path == "/api/settings")
        {
            string requestBody = context.Request.HttpMethod == "POST" ? ReadBody(context.Request) : null;
            string json = HandleSettingsApiRequest(context.Request.HttpMethod, requestBody);
            WriteJson(context.Response, json);
            return;
        }

        context.Response.StatusCode = 404;
        WriteText(context.Response, "Not found", "text/plain");
    }


    internal string HandleSettingsApiRequest(string httpMethod, string requestBody)
    {
        return InvokeOnMainThread(() => ExecuteSettingsApiActionImmediately(httpMethod, requestBody));
    }

    public string HandleSettingsApiRequestImmediately(string httpMethod, string requestBody)
    {
        return ExecuteSettingsApiActionImmediately(httpMethod, requestBody);
    }

    private string ExecuteSettingsApiActionImmediately(string httpMethod, string requestBody)
    {
        if (httpMethod == "GET")
        {
            return settingsBridge != null ? settingsBridge.GetSettingsJson() : WebUiSettingsStore.ToJson(WebUiSettingsStore.Load());
        }

        if (httpMethod == "POST")
        {
            WebUiSettingsData settings = settingsBridge != null
                ? settingsBridge.SaveSettingsFromJson(requestBody)
                : WebUiSettingsStore.FromJson(requestBody);

            if (settingsBridge == null)
            {
                WebUiSettingsStore.Save(settings);
            }

            return WebUiSettingsStore.ToJson(settings);
        }

        return "{}";
    }
    private string InvokeOnMainThread(Func<string> action)
    {
        if (action == null)
        {
            return "{}";
        }

        var invocation = new MainThreadInvocation
        {
            Action = action,
            WaitHandle = new ManualResetEventSlim(false)
        };

        lock (_queueLock)
        {
            _mainThreadQueue.Enqueue(invocation);
        }

        invocation.WaitHandle.Wait();
        invocation.WaitHandle.Dispose();

        if (invocation.Exception != null)
        {
            throw invocation.Exception;
        }

        return invocation.Result;
    }

    private void ProcessMainThreadQueue()
    {
        while (true)
        {
            MainThreadInvocation invocation;
            lock (_queueLock)
            {
                if (_mainThreadQueue.Count == 0)
                {
                    return;
                }

                invocation = _mainThreadQueue.Dequeue();
            }

            try
            {
                invocation.Result = invocation.Action != null ? invocation.Action() : "{}";
            }
            catch (Exception ex)
            {
                invocation.Exception = ex;
            }
            finally
            {
                invocation.WaitHandle.Set();
            }
        }
    }

    private void WriteHtml(HttpListenerResponse response)
    {
        WritePayload(response, _cachedHtmlBytes ?? Array.Empty<byte>(), "text/html");
    }

    private bool TryWriteStaticFile(string path, HttpListenerResponse response)
    {
        if (string.IsNullOrEmpty(path) || path == "/")
        {
            return false;
        }

        string relativePath = path.TrimStart('/');
        if (string.IsNullOrEmpty(relativePath))
        {
            return false;
        }

        string baseDirectory = !string.IsNullOrEmpty(_persistentWebUiFilePath)
            ? Path.GetDirectoryName(_persistentWebUiFilePath)
            : null;

        if (string.IsNullOrEmpty(baseDirectory) || !Directory.Exists(baseDirectory))
        {
            return false;
        }

        string resolvedPath = Path.GetFullPath(Path.Combine(baseDirectory, relativePath));
        string resolvedBaseDirectory = Path.GetFullPath(baseDirectory);
        if (!resolvedPath.StartsWith(resolvedBaseDirectory, StringComparison.Ordinal))
        {
            return false;
        }

        if (!File.Exists(resolvedPath))
        {
            return false;
        }

        byte[] payload = File.ReadAllBytes(resolvedPath);
        WritePayload(response, payload, GetMimeTypeFromPath(resolvedPath));
        return true;
    }

    private static string GetMimeTypeFromPath(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();
        switch (extension)
        {
            case ".css":
                return "text/css";
            case ".js":
                return "application/javascript";
            case ".json":
                return "application/json";
            case ".png":
                return "image/png";
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";
            case ".svg":
                return "image/svg+xml";
            default:
                return "application/octet-stream";
        }
    }

    private static string ReadBody(HttpListenerRequest request)
    {
        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
        {
            return reader.ReadToEnd();
        }
    }

    private static void WriteJson(HttpListenerResponse response, string json)
    {
        WriteText(response, json, "application/json");
    }

    private static void WriteText(HttpListenerResponse response, string content, string contentType)
    {
        byte[] payload = Encoding.UTF8.GetBytes(content);
        WritePayload(response, payload, contentType);
    }

    private static void WritePayload(HttpListenerResponse response, byte[] payload, string contentType)
    {
        response.ContentType = contentType;
        response.ContentEncoding = Encoding.UTF8;
        response.ContentLength64 = payload.Length;
        response.OutputStream.Write(payload, 0, payload.Length);
        SafeClose(response);
    }

    private static void SafeClose(HttpListenerResponse response)
    {
        try
        {
            response.OutputStream.Close();
            response.Close();
        }
        catch (Exception)
        {
            // Ignore close exceptions.
        }
    }
}
