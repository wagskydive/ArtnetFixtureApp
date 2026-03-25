using UnityEngine;
using UnityEngine.UI;

public class LockedCapabilityPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Text titleText;
    [SerializeField] private Text descriptionText;

    public void Show(string capabilityId, CapabilityDefinition definition)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = definition != null && !string.IsNullOrWhiteSpace(definition.DisplayTitle)
                ? definition.DisplayTitle
                : "Premium feature";
        }

        if (descriptionText != null)
        {
            descriptionText.text = definition != null && !string.IsNullOrWhiteSpace(definition.DisplayDescription)
                ? definition.DisplayDescription
                : $"Capability '{capabilityId}' is currently locked.";
        }
    }

    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }
}
