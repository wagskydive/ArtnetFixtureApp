using UnityEngine;
using UnityEngine.UI;

public class IapPurchasePanelItem : MonoBehaviour
{
    [SerializeField] private Text titleText;
    [SerializeField] private Text statusText;
    [SerializeField] private Text priceText;
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
        bool isConsumable = _definition != null && _definition.IsConsumable;
        RefreshPriceText();
        if (statusText != null)
        {
            if (isConsumable)
            {
                int purchaseCount = _panel != null ? _panel.GetConsumablePurchaseCount(_definition) : 0;
                statusText.text = $"Purchased: {purchaseCount}";
            }
            else
            {
                statusText.text = unlocked ? "Unlocked" : "Locked";
            }
        }

        if (purchaseButton != null)
        {
            purchaseButton.interactable = isConsumable || !unlocked;
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

    public void RefreshPriceText()
    {
        if (priceText == null)
        {
            return;
        }

        string displayPrice = _panel != null ? _panel.GetDisplayPrice(_definition) : string.Empty;
        priceText.text = string.IsNullOrWhiteSpace(displayPrice) ? "-" : displayPrice;
    }
}
