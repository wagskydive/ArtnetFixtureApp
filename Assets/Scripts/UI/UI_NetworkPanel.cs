using UnityEngine;
using UnityEngine.UI;

public class UI_NetworkPanel : MonoBehaviour
{
    private const string AdvancedNetworkingCapabilityId = "capability.advanced.networking";

    [SerializeField] private GameObject networkPanelRoot;
    [SerializeField] private GameObject sAcnSettingsRoot;
    [SerializeField] private NetworkingModeManager networkingModeManager;
    [SerializeField] private LockedCapabilityPanel lockedCapabilityPanel;
    
    [SerializeField] private Text networkModeText;

    void OnEnable()
    {
        OpenPanel();
    }

    public void OpenPanel()
    {
        if (!IsAdvancedNetworkingUnlocked())
        {
            if (lockedCapabilityPanel != null)
            {
                CapabilityDefinition definition = null;
                CapabilityService.Instance?.TryGetCapability(AdvancedNetworkingCapabilityId, out definition);
                lockedCapabilityPanel.Show(AdvancedNetworkingCapabilityId, definition);
            }

            if (networkPanelRoot != null)
            {
                networkPanelRoot.SetActive(false);
            }

            return;
        }

        if (networkPanelRoot != null)
        {
            networkPanelRoot.SetActive(true);
        }

        RefreshModeVisibility();
    }

    public void ClosePanel()
    {
        if (networkPanelRoot != null)
        {
            networkPanelRoot.SetActive(false);
        }
    }

    public void SetNetworkingMode(int modeIndex)
    {
        networkingModeManager?.SetModeFromIndex(modeIndex);
        RefreshModeVisibility();
    }

    public void SetModeToArtNet()
    {
        SetNetworkingMode(0);
    }

    public void SetModeToSAcn()
    {
        SetNetworkingMode(1);
    }

    public void RefreshModeVisibility()
    {
        if (sAcnSettingsRoot == null || networkingModeManager == null)
        {
            return;
        }

        sAcnSettingsRoot.SetActive(networkingModeManager.IsSAcnMode);
        if(networkingModeManager.IsSAcnMode)
        {
            networkModeText.text = "sACN";
        }
        else
        {
            networkModeText.text = "Art-Net";
        }
        
    }

    private static bool IsAdvancedNetworkingUnlocked()
    {
        return CapabilityService.Instance != null
            && CapabilityService.Instance.ResolveBoolean(AdvancedNetworkingCapabilityId, false);
    }
}
