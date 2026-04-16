using System;
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
        DmxSettingsBus.OnChanged += HandleDmxSettingsChanged;
        DmxSettingsService.OnLoaded += HandleDmxSettingsChanged;
    }
    void OnDisable()
    {
        DmxSettingsBus.OnChanged -= HandleDmxSettingsChanged;
        DmxSettingsService.OnLoaded -= HandleDmxSettingsChanged;

    }

    private void HandleDmxSettingsChanged(DmxSettingsSnapshot snapshot)
    {
        RefreshModeVisibility();
    }

    public void OpenPanel()
    {
        RefreshModeVisibility();
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
        DmxSettingsSnapshot snapshot = new DmxSettingsSnapshot(modeIndex == 1, DmxSettingsService.Instance.CurrentDmxSettings);
        SaveLoadSettings.SaveDmxSettings(snapshot);
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
        bool isSAcn = DmxSettingsService.Instance.CurrentDmxSettings.IsSAcnMode;

        sAcnSettingsRoot.SetActive(isSAcn);
        if (isSAcn)
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
