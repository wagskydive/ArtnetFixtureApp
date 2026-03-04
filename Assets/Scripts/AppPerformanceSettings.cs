using UnityEngine;

public class AppPerformanceSettings : MonoBehaviour
{
    [SerializeField] [Range(15, 60)] private int targetFrameRate = 30;
    [SerializeField] private bool disableVSync = true;

    private void Awake()
    {
        Apply();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        Apply();
    }

    public void Apply()
    {
        if (disableVSync)
        {
            QualitySettings.vSyncCount = 0;
        }

        Application.targetFrameRate = targetFrameRate;
    }
}
