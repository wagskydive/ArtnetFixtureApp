using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using MobileVersionCode;

public class UpdateChecker : MonoBehaviour
{
    [SerializeField] private string versionEndpoint = "https://dilarium.es/dmx-projector/api/version";
    [SerializeField] private GameObject updatePopup;

    private bool _checkedThisSession;

    void OnEnable()
    {
        UI_SettingsPanelToggle.OnMenuShown += CheckForUpdate;
    }

    void OnDisable()
    {
        UI_SettingsPanelToggle.OnMenuShown -= CheckForUpdate;
    }

    public void CheckForUpdate()
    {
        if (_checkedThisSession) return;

        StartCoroutine(CheckRoutine());
    }

    private IEnumerator CheckRoutine()
    {
        using (UnityWebRequest req = UnityWebRequest.Get(versionEndpoint))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Update check failed");
                yield break;
            }

            VersionResponse res = JsonUtility.FromJson<VersionResponse>(req.downloadHandler.text);

            int currentVersion = GetAndroidVersionCode();

            Debug.Log($"Version check → Current: {currentVersion}, Latest: {res.latestVersionCode}");

            if (res.latestVersionCode > currentVersion)
            {
                ShowUpdatePopup();
            }

            _checkedThisSession = true;
        }
    }

    private void ShowUpdatePopup()
    {
        if (updatePopup != null)
        {
            updatePopup.SetActive(true);
        }
    }

    public static void OpenStorePage()
    {
        Debug.Log("Opening Store Page");
        string packageName = "com.Dilarium.dmxProjector";

#if UNITY_ANDROID
        Application.OpenURL($"market://details?id={packageName}");
#else
        Application.OpenURL($"https://play.google.com/store/apps/details?id={packageName}");
#endif
    }

    private int GetAndroidVersionCode()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return VersionCode.GetVersionCode();
#else
        return 1;
#endif
    }

    [System.Serializable]
    private class VersionResponse
    {
        public int latestVersionCode;
        public int minSupportedVersionCode;
    }
}