using UnityEngine;

public class InAppWebViewSurface : MonoBehaviour
{
    [SerializeField] private LocalWebUiServer webUiServer;
    [SerializeField] private string pagePath = "/index.html?local=true&tv=true";
    [SerializeField] private bool openExternalBrowserInEditor = true;
    [SerializeField] private bool transparentOverlay = true;
    [SerializeField] private Vector2 overlayPositionNormalized = Vector2.zero;
    [SerializeField] private Vector2 overlaySizeNormalized = Vector2.one;

#if UNITY_EDITOR
    private bool _editorBrowserOpened;
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject _webView;
    private AndroidJavaObject _activity;
#endif

    private void Start()
    {
        InitializeWebView();
        SetVisible(false);
    }

    private void OnDestroy()
    {
        DestroyWebView();
    }

    public void SetVisible(bool visible)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_webView == null)
        {
            return;
        }

        int visibility = visible ? 0 : 8;
        _activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            _webView.Call("setVisibility", visibility);
        }));
#else
        if (visible)
        {
            OpenEditorPreview();
        }
#endif
    }

    public string GetWebUiUrl()
    {
        return BuildWebUiUrl();
    }

    private void InitializeWebView()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_webView != null)
        {
            return;
        }

        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            _activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }

        _activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            _webView = new AndroidJavaObject("android.webkit.WebView", _activity);
            AndroidJavaObject webSettings = _webView.Call<AndroidJavaObject>("getSettings");
            webSettings.Call("setJavaScriptEnabled", true);
            webSettings.Call("setDomStorageEnabled", true);
            webSettings.Call("setAllowFileAccess", true);

            if (transparentOverlay)
            {
                _webView.Call("setBackgroundColor", unchecked((int)0x00000000));
            }
            else
            {
                _webView.Call("setBackgroundColor", unchecked((int)0xFF000000));
            }

            using (var colorDrawable = new AndroidJavaObject("android.graphics.drawable.ColorDrawable", transparentOverlay ? unchecked((int)0x00000000) : unchecked((int)0xFF000000)))
            {
                _webView.Call("setBackground", colorDrawable);
            }

            using (AndroidJavaObject layoutParams = BuildLayoutParams())
            {
                _activity.Call("addContentView", _webView, layoutParams);
            }

            _webView.Call("loadUrl", BuildWebUiUrl());
        }));
#elif UNITY_EDITOR
        if (openExternalBrowserInEditor)
        {
            OpenEditorPreview();
        }
#endif
    }

    private void DestroyWebView()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_webView == null || _activity == null)
        {
            return;
        }

        _activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            if (_webView == null)
            {
                return;
            }

            _webView.Call("stopLoading");
            _webView.Call("destroy");
            _webView.Dispose();
            _webView = null;
        }));
#endif
    }

    private string BuildWebUiUrl()
    {
        int port = webUiServer != null ? webUiServer.Port : 8080;
        return $"http://127.0.0.1:{port}{pagePath}";
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject BuildLayoutParams()
    {
        using (var metrics = new AndroidJavaObject("android.util.DisplayMetrics"))
        {
            AndroidJavaObject windowManager = _activity.Call<AndroidJavaObject>("getWindowManager");
            AndroidJavaObject display = windowManager.Call<AndroidJavaObject>("getDefaultDisplay");
            display.Call("getRealMetrics", metrics);

            int screenWidth = metrics.Get<int>("widthPixels");
            int screenHeight = metrics.Get<int>("heightPixels");

            Vector2 clampedPosition = new Vector2(
                Mathf.Clamp01(overlayPositionNormalized.x),
                Mathf.Clamp01(overlayPositionNormalized.y));

            Vector2 clampedSize = new Vector2(
                Mathf.Clamp01(overlaySizeNormalized.x),
                Mathf.Clamp01(overlaySizeNormalized.y));

            clampedSize.x = Mathf.Max(clampedSize.x, 0.05f);
            clampedSize.y = Mathf.Max(clampedSize.y, 0.05f);

            int width = Mathf.Clamp(Mathf.RoundToInt(screenWidth * clampedSize.x), 1, screenWidth);
            int height = Mathf.Clamp(Mathf.RoundToInt(screenHeight * clampedSize.y), 1, screenHeight);

            int left = Mathf.Clamp(Mathf.RoundToInt(screenWidth * clampedPosition.x), 0, screenWidth - width);
            int top = Mathf.Clamp(Mathf.RoundToInt(screenHeight * clampedPosition.y), 0, screenHeight - height);

            var layoutParams = new AndroidJavaObject("android.widget.FrameLayout$LayoutParams", width, height);
            layoutParams.Set<int>("leftMargin", left);
            layoutParams.Set<int>("topMargin", top);
            return layoutParams;
        }
    }
#endif

#if UNITY_EDITOR
    private void OpenEditorPreview()
    {
        string url = BuildWebUiUrl();

        if (!_editorBrowserOpened && openExternalBrowserInEditor)
        {
            Application.OpenURL(url);
            _editorBrowserOpened = true;
        }

        Debug.Log($"InAppWebViewSurface (Editor) preview URL: {url}. Overlay pos={overlayPositionNormalized} size={overlaySizeNormalized} transparent={transparentOverlay}");
    }
#endif
}
