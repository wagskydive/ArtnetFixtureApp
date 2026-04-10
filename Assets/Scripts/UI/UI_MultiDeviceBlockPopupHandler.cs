using System;
using UnityEngine;

public class UI_MultiDeviceBlockPopupHandler : MonoBehaviour
{
    [SerializeField]
    GameObject blockPopup;


    void Start()
    {
        MultiDeviceBridge.Instance.OnMultiDeviceBlocked += ShowPopup;
        MultiDeviceBridge.Instance.OnMultiDeviceBlocked += HidePopup;

    }

    private void HidePopup()
    {
        blockPopup.SetActive(true);
    }

    private void ShowPopup()
    {
        blockPopup.SetActive(false);
    }
}
