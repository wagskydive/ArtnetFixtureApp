using UnityEngine;

public class InfoTextIAP : MonoBehaviour, IInfoText
{


    public string GetInfoText()
    {
        IapPurchasePanelItem iapPurchasePanelItem = GetComponentInParent<IapPurchasePanelItem>();
        if(iapPurchasePanelItem != null)
        {
            return iapPurchasePanelItem.GetInfoText();
        }

        return "";
    }

}
