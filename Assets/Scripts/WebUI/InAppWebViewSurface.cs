using UnityEngine;

public class InAppWebViewSurface : MonoBehaviour
{
    [SerializeField] private LocalWebUiServer webUiServer;
    [SerializeField] private string pagePath = "/index.html?local=true&tv=true";
    [SerializeField] private bool openExternalBrowserInEditor = true;

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

            _webView.Call("setBackgroundColor", unchecked((int)0xFF000000));

            using (var layoutParams = new AndroidJavaObject("android.view.ViewGroup$LayoutParams", -1, -1))
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

    public string GetWebUiUrl()
    {
        return BuildWebUiUrl();
    }

    private string BuildWebUiUrl()
    {
        int port = webUiServer != null ? webUiServer.Port : 8080;
        return $"http://127.0.0.1:{port}{pagePath}";
    }

#if UNITY_EDITOR
    private void OpenEditorPreview()
    {
        string url = BuildWebUiUrl();

        if (!_editorBrowserOpened && openExternalBrowserInEditor)
        {
            Application.OpenURL(url);
            _editorBrowserOpened = true;
        }

        Debug.Log($"InAppWebViewSurface (Editor) preview URL: {url}");
    }
#endif
}
