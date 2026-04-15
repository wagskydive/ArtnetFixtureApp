using UnityEngine;

public class DmxBootstrap : MonoBehaviour
{
    void Start()
    {
        DmxSettingsService.Instance.Load();
    }
}