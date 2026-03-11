using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

public class LocalWebUiServer : MonoBehaviour
{
    private class MainThreadInvocation
    {
        public Func<string> Action;
        public ManualResetEventSlim WaitHandle;
        public string Result;
        public Exception Exception;
    }

    [SerializeField] private TextAsset webUiHtml;
    [SerializeField] private WebUiSettingsBridge settingsBridge;
    [SerializeField] private int port = 8080;

    private readonly Queue<MainThreadInvocation> _mainThreadQueue = new Queue<MainThreadInvocation>(8);
    private readonly object _queueLock = new object();

    private HttpListener _listener;
    private Thread _serverThread;
    private volatile bool _isRunning;
    private byte[] _cachedHtmlBytes;

    public int Port => port;

    private void Awake()
    {
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
        string html = webUiHtml != null ? webUiHtml.text : "<html><body>Missing webUiHtml reference.</body></html>";
        _cachedHtmlBytes = Encoding.UTF8.GetBytes(html);
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

        if (path == "/api/settings" && context.Request.HttpMethod == "GET")
        {
            string json = InvokeOnMainThread(() =>
            {
                return settingsBridge != null ? settingsBridge.GetSettingsJson() : WebUiSettingsStore.ToJson(WebUiSettingsStore.Load());
            });
            WriteJson(context.Response, json);
            return;
        }

        if (path == "/api/settings" && context.Request.HttpMethod == "POST")
        {
            string requestBody = ReadBody(context.Request);
            string json = InvokeOnMainThread(() =>
            {
                WebUiSettingsData settings = settingsBridge != null
                    ? settingsBridge.SaveSettingsFromJson(requestBody)
                    : WebUiSettingsStore.FromJson(requestBody);

                if (settingsBridge == null)
                {
                    WebUiSettingsStore.Save(settings);
                }

                return WebUiSettingsStore.ToJson(settings);
            });

            WriteJson(context.Response, json);
            return;
        }

        context.Response.StatusCode = 404;
        WriteText(context.Response, "Not found", "text/plain");
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
