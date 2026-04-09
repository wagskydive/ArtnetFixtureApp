using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UI_DpadNavigationController))]
public class UI_CancelButtonBlocker : MonoBehaviour
{
    List<Popup> activePopups = new List<Popup>();

    UI_DpadNavigationController uI_DpadNavigationController;
    void Awake()
    {
        uI_DpadNavigationController = GetComponent<UI_DpadNavigationController>();
        Popup.OnPopupNavigationBlocked += AddActivePopup;
        Popup.OnPopupNavigationBlockReleased += RemoveActivePopup;
    }

    void AddActivePopup(Popup activePopup)
    {
        activePopups.Add(activePopup);
        uI_DpadNavigationController.SetCancelButtonBlock(true);
    }

    void RemoveActivePopup(Popup deactivatedPoup)
    {
        if (activePopups.Contains(deactivatedPoup))
        {
            activePopups.Remove(deactivatedPoup);
            if(activePopups.Count == 0)
            {
                uI_DpadNavigationController.SetCancelButtonBlock(false);
                uI_DpadNavigationController.SetLastCancelFrame(Time.frameCount);
            }
        }
    }

    void OnDisable()
    {
        activePopups = new List<Popup>();
    }

}
