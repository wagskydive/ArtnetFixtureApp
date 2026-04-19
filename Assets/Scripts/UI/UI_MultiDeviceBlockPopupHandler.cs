using System;
using UnityEngine;

public class UI_MultiDeviceBlockPopupHandler : MonoBehaviour
{
    [SerializeField]
    GameObject blockPopup;


    void OnEnable()
    {
        MultiDeviceBridge.Instance.OnMultiDeviceStateIsBlocked += ShowPopup;
        MultiDeviceBridge.Instance.OnMultiDeviceStateIsUnblocked += HidePopup;


        // ✅ Sync immediately
        if (MultiDeviceBridge.Instance.IsCurrentlyBlocked())
            ShowPopup();
        else
            HidePopup();
    }

    void OnDisable()
    {
        MultiDeviceBridge.Instance.OnMultiDeviceStateIsBlocked -= ShowPopup;
        MultiDeviceBridge.Instance.OnMultiDeviceStateIsUnblocked -= HidePopup;
    }

    private void HidePopup()
    {
        blockPopup.SetActive(false);
    }

    private void ShowPopup()
    {
        blockPopup.SetActive(true);
    }
}
