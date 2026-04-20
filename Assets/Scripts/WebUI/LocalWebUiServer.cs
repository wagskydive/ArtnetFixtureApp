using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Globalization;
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

    [Serializable]
    private class WebUiAuthRequest
    {
        public string password;
    }

    [Serializable]
    private class WebUiAuthResponse
    {
        public bool authenticated;
    }

    [Serializable]
    private class GoboSlotState
    {
        public int slot;
        public string fileName;
        public bool hasImage;
        public string previewUrl;
    }

    [Serializable]
    private class GoboSlotStateList
    {
        public GoboSlotState[] slots;
        public bool unlocked;
    }

    [Serializable]
    private class GoboUploadResponse
    {
        public bool success;
        public string message;
        public int slot;
    }

    private const string CustomGoboProductId = "custom.gobos.upgrade";
    private const string CustomGoboCapabilityId = "capability.custom.gobos";

    [SerializeField] private TextAsset webUiHtml;
    [SerializeField] private WebUiSettingsBridge settingsBridge;
    [SerializeField] private int port = 8080;

    private readonly Queue<MainThreadInvocation> _mainThreadQueue = new Queue<MainThreadInvocation>(8);
    private readonly object _queueLock = new object();

    private HttpListener _listener;
    private Thread _serverThread;
    private volatile bool _isRunning;
    private byte[] _cachedHtmlBytes;
    private string _serverSessionId;

    public int Port => port;

    private void Awake()
    {
        WebUiPasswordProtection.MigrateLegacyPasswordIfNeeded();
        _serverSessionId = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
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

        if (path == "/api/settings")
        {
            string requestBody = context.Request.HttpMethod == "POST" ? ReadBody(context.Request) : null;
            string json = HandleSettingsApiRequest(context.Request.HttpMethod, requestBody);
            WriteJson(context.Response, json);
            return;
        }

        if (path == "/api/login")
        {
            string requestBody = context.Request.HttpMethod == "POST" ? ReadBody(context.Request) : null;
            string json = HandleLoginApiRequest(context.Request.HttpMethod, requestBody);
            WriteJson(context.Response, json);
            return;
        }

        if (path == "/api/network-debug")
        {
            string json = HandleNetworkDebugApiRequest(context.Request.HttpMethod);
            WriteJson(context.Response, json);
            return;
        }

        if (path == "/images")
        {
            string json = HandleImagesApiRequest(context.Request.HttpMethod);
            WriteJson(context.Response, json);
            return;
        }

        if (path.StartsWith("/CustomGobos/", StringComparison.OrdinalIgnoreCase))
        {
            WriteCustomGoboPreview(context.Request, context.Response);
            return;
        }

        if (path == "/upload")
        {
            HandleUploadRequest(context.Request, context.Response);
            return;
        }

        if (path == "/remove")
        {
            HandleRemoveRequest(context.Request, context.Response);
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
            WebUiSettingsData loaded = settingsBridge != null ? settingsBridge.GetSettings() : WebUiSettingsStore.Load();
            loaded.serverSessionId = _serverSessionId;
            loaded.advancedNetworkingUnlocked = IsAdvancedNetworkingUnlocked();
            loaded.ipAddress = GetLocalIpv4Address();
            loaded.passwordConfigured = WebUiPasswordProtection.HasConfiguredPassword();
            loaded.passwordEnabled = WebUiPasswordProtection.IsEnabled();
            return WebUiSettingsStore.ToJson(loaded);
        }

        if (httpMethod == "POST")
        {
            WebUiSettingsData request = WebUiSettingsStore.FromJson(requestBody);
            if (!string.Equals(request.serverSessionId, _serverSessionId, StringComparison.Ordinal))
            {
                Debug.Log("LocalWebUiServer ignored stale WebUI settings POST due to mismatched serverSessionId.");
                WebUiSettingsData fresh = settingsBridge != null ? settingsBridge.GetSettings() : WebUiSettingsStore.Load();
                fresh.serverSessionId = _serverSessionId;
                fresh.advancedNetworkingUnlocked = IsAdvancedNetworkingUnlocked();
                fresh.ipAddress = GetLocalIpv4Address();
                fresh.passwordConfigured = WebUiPasswordProtection.HasConfiguredPassword();
                fresh.passwordEnabled = WebUiPasswordProtection.IsEnabled();
                return WebUiSettingsStore.ToJson(fresh);
            }

            WebUiSettingsData settings = settingsBridge != null
                ? settingsBridge.SaveSettingsFromJson(WebUiSettingsStore.ToJson(request))
                : request;

            if (settingsBridge == null)
            {
                WebUiSettingsStore.Save(settings);
            }

            settings.serverSessionId = _serverSessionId;
            settings.advancedNetworkingUnlocked = IsAdvancedNetworkingUnlocked();
            settings.ipAddress = GetLocalIpv4Address();
            settings.passwordConfigured = WebUiPasswordProtection.HasConfiguredPassword();
            settings.passwordEnabled = WebUiPasswordProtection.IsEnabled();
            return WebUiSettingsStore.ToJson(settings);
        }

        return "{}";
    }

    internal string HandleLoginApiRequest(string httpMethod, string requestBody)
    {
        return InvokeOnMainThread(() => ExecuteLoginApiActionImmediately(httpMethod, requestBody));
    }

    public string HandleLoginApiRequestImmediately(string httpMethod, string requestBody)
    {
        return ExecuteLoginApiActionImmediately(httpMethod, requestBody);
    }

    internal string HandleImagesApiRequest(string httpMethod)
    {
        return InvokeOnMainThread(() => ExecuteImagesApiActionImmediately(httpMethod));
    }

    public string HandleImagesApiRequestImmediately(string httpMethod)
    {
        return ExecuteImagesApiActionImmediately(httpMethod);
    }

    internal string HandleNetworkDebugApiRequest(string httpMethod)
    {
        return InvokeOnMainThread(() => ExecuteNetworkDebugApiActionImmediately(httpMethod));
    }

    public string HandleNetworkDebugApiRequestImmediately(string httpMethod)
    {
        return ExecuteNetworkDebugApiActionImmediately(httpMethod);
    }

    private string ExecuteNetworkDebugApiActionImmediately(string httpMethod)
    {
        if (httpMethod != "GET")
        {
            return "{}";
        }

        NetworkDebugService.NetworkDebugSnapshot snapshot = NetworkDebugService.Instance != null
            ? NetworkDebugService.Instance.BuildSnapshot()
            : new NetworkDebugService.NetworkDebugSnapshot();

        return JsonUtility.ToJson(snapshot);
    }

    private string ExecuteImagesApiActionImmediately(string httpMethod)
    {
        if (httpMethod != "GET")
        {
            return JsonUtility.ToJson(new GoboSlotStateList { slots = Array.Empty<GoboSlotState>(), unlocked = false });
        }

        bool unlocked = IsCustomGoboUpgradeUnlocked();
        var slots = new GoboSlotState[CustomGoboStorage.MaxSlots];
        for (int slot = 1; slot <= CustomGoboStorage.MaxSlots; slot++)
        {
            string fileName = CustomGoboStorage.GetSlotFileName(slot);
            bool hasImage = File.Exists(CustomGoboStorage.GetSlotPath(slot));
            slots[slot - 1] = new GoboSlotState
            {
                slot = slot,
                fileName = fileName,
                hasImage = hasImage,
                previewUrl = hasImage ? $"/CustomGobos/{fileName}?t={DateTime.UtcNow.Ticks}" : string.Empty
            };
        }

        return JsonUtility.ToJson(new GoboSlotStateList
        {
            slots = slots,
            unlocked = unlocked
        });
    }

    private void HandleUploadRequest(HttpListenerRequest request, HttpListenerResponse response)
    {
        if (!string.Equals(request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        {
            response.StatusCode = 405;
            WriteJson(response, JsonUtility.ToJson(new GoboUploadResponse
            {
                success = false,
                message = "POST is required.",
                slot = 0
            }));
            return;
        }

        int slot = ParseSlot(request.QueryString["slot"]);
        byte[] payload = ReadUploadPayload(request);

        string result = InvokeOnMainThread(() =>
        {
            if (!IsCustomGoboUpgradeUnlocked())
            {
                return JsonUtility.ToJson(new GoboUploadResponse
                {
                    success = false,
                    message = "Custom gobo upgrade not unlocked.",
                    slot = slot
                });
            }

            bool saved = CustomGoboStorage.TrySaveSlotPng(slot, payload, out string error);
            return JsonUtility.ToJson(new GoboUploadResponse
            {
                success = saved,
                message = saved ? "Upload successful." : error,
                slot = slot
            });
        });

        GoboUploadResponse parsed = JsonUtility.FromJson<GoboUploadResponse>(result);
        response.StatusCode = parsed != null && parsed.success ? 200 : 400;
        WriteJson(response, result);
    }

    private void HandleRemoveRequest(HttpListenerRequest request, HttpListenerResponse response)
    {
        if (!string.Equals(request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        {
            response.StatusCode = 405;
            WriteJson(response, JsonUtility.ToJson(new GoboUploadResponse
            {
                success = false,
                message = "POST is required.",
                slot = 0
            }));
            return;
        }

        int slot = ParseSlot(request.QueryString["slot"]);
        string result = InvokeOnMainThread(() =>
        {
            if (!IsCustomGoboUpgradeUnlocked())
            {
                return JsonUtility.ToJson(new GoboUploadResponse
                {
                    success = false,
                    message = "Custom gobo upgrade not unlocked.",
                    slot = slot
                });
            }

            bool removed = CustomGoboStorage.TryDeleteSlotAndCompact(slot, out string error);
            return JsonUtility.ToJson(new GoboUploadResponse
            {
                success = removed,
                message = removed ? "Slot removed." : error,
                slot = slot
            });
        });

        GoboUploadResponse parsed = JsonUtility.FromJson<GoboUploadResponse>(result);
        bool locked = parsed != null && !parsed.success && parsed.message == "Custom gobo upgrade not unlocked.";
        response.StatusCode = parsed != null && parsed.success ? 200 : (locked ? 403 : 400);
        WriteJson(response, result);
    }

    private void WriteCustomGoboPreview(HttpListenerRequest request, HttpListenerResponse response)
    {
        string path = request.Url != null ? request.Url.AbsolutePath : string.Empty;
        string fileName = path.Replace("/CustomGobos/", string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(fileName) || !Regex.IsMatch(fileName, "^slot([1-9]|1[0-6])\\.png$", RegexOptions.IgnoreCase))
        {
            response.StatusCode = 404;
            WriteText(response, "Not found", "text/plain");
            return;
        }

        string fullPath = Path.Combine(CustomGoboStorage.GetFolderPath(), fileName.ToLowerInvariant());
        if (!File.Exists(fullPath))
        {
            response.StatusCode = 404;
            WriteText(response, "Not found", "text/plain");
            return;
        }

        byte[] data = File.ReadAllBytes(fullPath);
        WritePayload(response, data, "image/png");
    }

    private string ExecuteLoginApiActionImmediately(string httpMethod, string requestBody)
    {
        if (httpMethod != "POST")
        {
            return JsonUtility.ToJson(new WebUiAuthResponse { authenticated = false });
        }

        WebUiAuthRequest request = string.IsNullOrWhiteSpace(requestBody)
            ? new WebUiAuthRequest()
            : JsonUtility.FromJson<WebUiAuthRequest>(requestBody);

        bool passwordEnabled = WebUiPasswordProtection.IsEnabled();
        bool passwordConfigured = WebUiPasswordProtection.HasConfiguredPassword();
        bool authenticated = !passwordEnabled || !passwordConfigured
            || WebUiPasswordProtection.VerifyPassword(request != null ? request.password : string.Empty);

        return JsonUtility.ToJson(new WebUiAuthResponse { authenticated = authenticated });
    }

    private bool IsCustomGoboUpgradeUnlocked()
    {
        if (CapabilityService.Instance == null)
        {
            return false;
        }

        bool unlockedByCapability = CapabilityService.Instance.ResolveBoolean(CustomGoboCapabilityId, false);
        if (unlockedByCapability)
        {
            return true;
        }

        return CapabilityService.Instance.Entitlements != null
               && CapabilityService.Instance.Entitlements.IsUnlocked(CustomGoboProductId);
    }

    private static bool IsAdvancedNetworkingUnlocked()
    {
        return CapabilityService.Instance != null
            && CapabilityService.Instance.ResolveBoolean("capability.advanced.networking", false);
    }

    private static int ParseSlot(string rawSlot)
    {
        if (!int.TryParse(rawSlot, out int slot))
        {
            return -1;
        }

        return slot;
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


    private static string GetLocalIpv4Address()
    {
        return IpSolver.ResolveLocalIpv4Address();
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

    private static byte[] ReadUploadPayload(HttpListenerRequest request)
    {
        if (request == null || request.InputStream == null)
        {
            return Array.Empty<byte>();
        }

        string contentType = request.ContentType ?? string.Empty;
        if (!contentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase))
        {
            using (var memory = new MemoryStream())
            {
                request.InputStream.CopyTo(memory);
                return memory.ToArray();
            }
        }

        string boundaryToken = "boundary=";
        int boundaryIndex = contentType.IndexOf(boundaryToken, StringComparison.OrdinalIgnoreCase);
        if (boundaryIndex < 0)
        {
            return Array.Empty<byte>();
        }

        string boundary = contentType.Substring(boundaryIndex + boundaryToken.Length).Trim().Trim('"');
        using (var memory = new MemoryStream())
        {
            request.InputStream.CopyTo(memory);
            return ExtractMultipartFile(memory.ToArray(), boundary);
        }
    }

    private static byte[] ExtractMultipartFile(byte[] payload, string boundary)
    {
        if (payload == null || payload.Length == 0 || string.IsNullOrWhiteSpace(boundary))
        {
            return Array.Empty<byte>();
        }

        byte[] boundaryBytes = Encoding.UTF8.GetBytes("--" + boundary);
        int firstBoundaryIndex = IndexOf(payload, boundaryBytes, 0);
        if (firstBoundaryIndex < 0)
        {
            return Array.Empty<byte>();
        }

        byte[] headerDelimiter = Encoding.UTF8.GetBytes("\r\n\r\n");
        int headerEnd = IndexOf(payload, headerDelimiter, firstBoundaryIndex);
        if (headerEnd < 0)
        {
            return Array.Empty<byte>();
        }

        int dataStart = headerEnd + headerDelimiter.Length;
        byte[] nextBoundaryPattern = Encoding.UTF8.GetBytes("\r\n--" + boundary);
        int dataEnd = IndexOf(payload, nextBoundaryPattern, dataStart);
        if (dataEnd < 0 || dataEnd <= dataStart)
        {
            return Array.Empty<byte>();
        }

        int length = dataEnd - dataStart;
        var fileBytes = new byte[length];
        Buffer.BlockCopy(payload, dataStart, fileBytes, 0, length);
        return fileBytes;
    }

    private static int IndexOf(byte[] haystack, byte[] needle, int startIndex)
    {
        if (haystack == null || needle == null || needle.Length == 0 || startIndex < 0)
        {
            return -1;
        }

        for (int i = startIndex; i <= haystack.Length - needle.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < needle.Length; j++)
            {
                if (haystack[i + j] != needle[j])
                {
                    match = false;
                    break;
                }
            }

            if (match)
            {
                return i;
            }
        }

        return -1;
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
