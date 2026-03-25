using UnityEngine;
using UnityEngine.UI;

public class IapPurchasePanelItem : MonoBehaviour
{
    [SerializeField] private Text titleText;
    [SerializeField] private Text statusText;
    [SerializeField] private Button purchaseButton;

    private CapabilityDefinition _definition;
    private IapPurchasePanel _panel;

    public void Bind(CapabilityDefinition definition, IapPurchasePanel panel)
    {
        _definition = definition;
        _panel = panel;

        if (purchaseButton != null)
        {
            purchaseButton.onClick.RemoveListener(HandlePurchaseClicked);
            purchaseButton.onClick.AddListener(HandlePurchaseClicked);
        }

        if (titleText != null)
        {
            titleText.text = !string.IsNullOrWhiteSpace(definition.DisplayTitle)
                ? definition.DisplayTitle
                : definition.Id;
        }



        bool unlocked = _panel != null && _panel.IsUnlocked(_definition);
        if (statusText != null)
        {
            statusText.text = unlocked ? "Unlocked" : "Locked";
        }

        if (purchaseButton != null)
        {
            purchaseButton.interactable = !unlocked;
        }
        if(unlocked)
        {
            purchaseButton.interactable = false;
            
        }
    }

    private void HandlePurchaseClicked()
    {
        _panel?.Purchase(_definition);
    }

    public string GetInfoText()
    {
        if (_definition != null)
        {
            return _definition.DisplayDescription;
        }

        return "";
    }
}
