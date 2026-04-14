using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Timers;

public class UI_DmxSettings : MonoBehaviour
{
    [SerializeField] private Text channelValueText;
    [SerializeField] private Text universeValueText;
    [SerializeField] private InputField channelInputField;
    [SerializeField] private InputField universeInputField;
    [SerializeField] private Text fixtureNameValueText;
    [SerializeField] private Text ipAddressValueText;
    [SerializeField] private GameObject passwordPanel;
    [SerializeField] private GameObject networkWarning;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private Toggle networkWarningToggle;
    [SerializeField] private Toggle infoPanelToggle;
    [SerializeField] private Toggle webUiPasswordEnabledToggle;
    [SerializeField] private Text webUiPasswordText;
    [SerializeField] private Text webUiPasswordPlaceholderText;
    [SerializeField] private Text webUiPasswordAstrisksText;
    [SerializeField] private Text webUiPasswordResetButtonText;
    [SerializeField] private int currentPatternType = 0; // Pattern type selector (0=Static, 1=Pulse, 2=ColorShift)
    private bool shouldDisplayNetworkWarning;
    [SerializeField] private UI_FixtureMeshManager fixtureMeshManager;
    [SerializeField] private CapabilityBlockUiTrigger capabilityBlockUiTrigger;
    [SerializeField] private CapabilityDefinition universeLimitCapability;

    public static UI_DmxSettings Instance { get; private set; }

    private int currentDmxChannel { get => SaveLoadSettings.LoadInt(SaveLoadSettings.DmxChannelKey, 1); }
    private int currentDmxUniverse { get => SaveLoadSettings.LoadInt(SaveLoadSettings.DmxUniverseKey, 1); }
    private bool hasLoadedPreferences;
    private bool isLoadingPreferences;
    private bool isApplicationQuitting;
    private INetworkReceiver _subscribedReceiver;

    public IShaderGlobalIntSetter ShaderGlobalIntSetter { get; set; } = new UnityShaderGlobalIntSetter();

    public int CurrentDmxChannel
    {
        get => currentDmxChannel;
    }

    public void SetCurrentDmxChannel(int value)
    {
        int clampedRequested = Mathf.Clamp(value, 1, 512);
        SaveLoadSettings.SaveInt(SaveLoadSettings.DmxChannelKey, clampedRequested);
        UpdateChannelDisplay();
    }

    public void SetCurrentDmxChannelFromUserInput(int value)
    {
        SetCurrentDmxChannel(value);
        SavePreferences();
    }

    public int CurrentDmxUniverse
    {
        get => currentDmxUniverse;
    }



    public void SetUniverse(int value)
    {
        int clamped = Mathf.Clamp(value, 1, GetMaxSelectableUniverse());
        SaveLoadSettings.SaveInt(SaveLoadSettings.DmxUniverseKey, clamped);
        SaveLoadSettings.SaveAndInvokeEvent();
    }

    public void SetUniverseFromUserInput(int universeToSet)
    {
        SetUniverse(universeToSet);
        //SavePreferences();
    }

    // Pattern type selector (0=Static, 1=Pulse, 2=ColorShift)
    public int CurrentPatternType
    {
        get => currentPatternType;
        set
        {
            currentPatternType = Mathf.Max(0, value);
            OnPatternTypeChanged();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        SyncReceiverSubscription();
        LoadPreferencesAndApply();
        SaveLoadSettings.OnSettingsSaved += UpdateDisplay;
        RefreshPasswordControls();
    }

    void ShowNetworkWarning()
    {
        shouldDisplayNetworkWarning = true;
        RefreshNetworkWarningVisibility();
    }

    void HideNetworkWarning()
    {
        shouldDisplayNetworkWarning = false;
        RefreshNetworkWarningVisibility();
    }

    private void OnDestroy()
    {
        SaveLoadSettings.OnSettingsSaved -= UpdateDisplay;

        if (_subscribedReceiver != null)
        {
            _subscribedReceiver.NoDataReceivedRecently -= ShowNetworkWarning;
            _subscribedReceiver.DataReceivedAgain -= HideNetworkWarning;
            _subscribedReceiver = null;
        }
    }

    private void Update()
    {
        SyncReceiverSubscription();
    }

    private void OnDisable()
    {
        if (!hasLoadedPreferences || isApplicationQuitting)
        {
            return;
        }

        SavePreferences();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SavePreferences();
        }
    }

    private void OnApplicationQuit()
    {
        isApplicationQuitting = true;
    }

    public void IncreaseChannel()
    {
        SetCurrentDmxChannelFromUserInput(Mathf.Min(512, CurrentDmxChannel + 1));

        SavePreferences();
    }

    public void DecreaseChannel()
    {
        SetCurrentDmxChannelFromUserInput(Mathf.Max(1, CurrentDmxChannel - 1));
        SavePreferences();
    }

    public void IncreaseUniverse()
    {
        SetUniverseFromUserInput(Mathf.Min(63999, CurrentDmxUniverse + 1));

        SavePreferences();
    }

    public void DecreaseUniverse()
    {
        SetUniverseFromUserInput(Mathf.Max(1, CurrentDmxUniverse - 1));
        SavePreferences();
    }

    public enum PatternType
    {
        Static,
        Pulse,
        ColorShift
    }

    public void SavePreferences()
    {
        if (isLoadingPreferences)
        {
            return;
        }
        SaveLoadSettings.SaveInt(SaveLoadSettings.DmxChannelKey, CurrentDmxChannel);
        SaveLoadSettings.SaveInt(SaveLoadSettings.DmxUniverseKey, CurrentDmxUniverse);
        SaveLoadSettings.SaveInt(SaveLoadSettings.DmxPatternKey, CurrentPatternType);
        SaveLoadSettings.SaveAndInvokeEvent();
    }

    void LoadSettingsWithoutApply()
    {
        LoadPreferences(false);
    }

    public void LoadPreferencesAndApply()
    {
        LoadPreferences(true);
    }




    public void LoadPreferences(bool apply)
    {
        isLoadingPreferences = true;

        // ❌ DON'T call SetUniverse / SetChannel here
        // they trigger saves again

        int universe = SaveLoadSettings.LoadInt(SaveLoadSettings.DmxUniverseKey, CurrentDmxUniverse);
        int channel = SaveLoadSettings.LoadInt(SaveLoadSettings.DmxChannelKey, CurrentDmxChannel);

        // just update UI
        UpdateDisplay();

        isLoadingPreferences = false;
        hasLoadedPreferences = true;
    }

    private void UpdateDisplay()
    {
        UpdateUniverseDisplay();
        UpdateChannelDisplay();
        UpdateDeviceInfoDisplay();
        UpdateWarningToggleState();
        UpdateInfoPanelState();
        RefreshPasswordControls();
    }


    private void UpdateChannelDisplay()
    {
        if (channelValueText != null)
        {
            channelValueText.text = CurrentDmxChannel.ToString();
        }
        if (channelInputField != null)
        {
            channelInputField.text = CurrentDmxChannel.ToString();
        }
    }

    private void UpdateUniverseDisplay()
    {
        if (universeValueText != null)
        {
            universeValueText.text = CurrentDmxUniverse.ToString();
        }
        if (universeInputField != null)
        {
            universeInputField.text = CurrentDmxUniverse.ToString();
        }
    }

    // Called whenever currentPatternType changes
    private void OnPatternTypeChanged()
    {
        // Update any visual output or shader based on currentPatternType
        // Example: Set shader global int (replace with your actual logic)
        ShaderGlobalIntSetter.SetGlobalInt("_PatternType", currentPatternType);
    }

    public void SetNetworkWarning(bool isOn)
    {
        SaveLoadSettings.SaveInt(SaveLoadSettings.NetworkWarningEnabledKey, isOn ? 1 : 0);
        SaveLoadSettings.SaveAndInvokeEvent();
        RefreshNetworkWarningVisibility();
    }

    private void UpdateWarningToggleState()
    {
        bool enabled = SaveLoadSettings.LoadInt(SaveLoadSettings.NetworkWarningEnabledKey, 1) == 1;
        if (networkWarningToggle != null)
        {
            networkWarningToggle.SetIsOnWithoutNotify(enabled);
        }

        RefreshNetworkWarningVisibility();
    }

    private void RefreshNetworkWarningVisibility()
    {
        if (networkWarning == null)
        {
            return;
        }

        bool warningEnabled = SaveLoadSettings.LoadInt(SaveLoadSettings.NetworkWarningEnabledKey, 1) == 1;
        networkWarning.SetActive(warningEnabled && shouldDisplayNetworkWarning);
    }

    private void UpdateInfoPanelState()
    {
        if (infoPanelToggle == null)
        {
            return;
        }

        bool enabled = SaveLoadSettings.LoadInt(SaveLoadSettings.InfoPanelEnabledKey, 1) == 1;
        infoPanelToggle.SetIsOnWithoutNotify(enabled);
        if (infoPanel != null)
        {
            infoPanel.SetActive(enabled);
        }
    }

    public void SetInfoPanelEnabled(bool isOn)
    {
        SaveLoadSettings.SaveInt(SaveLoadSettings.InfoPanelEnabledKey, isOn ? 1 : 0);
        SaveLoadSettings.SaveAndInvokeEvent();

        if (infoPanel != null)
        {
            infoPanel.SetActive(isOn);
        }
    }

    public void SetWebUiPassword(string value)
    {
        WebUiPasswordProtection.SetPassword(value);
        RefreshPasswordControls();
        SaveLoadSettings.SaveAndInvokeEvent();
    }

    public void ApplyWebUiPasswordFromInput()
    {
        string value = webUiPasswordText != null ? webUiPasswordText.text : string.Empty;
        SetWebUiPassword(value);
    }

    public void OnWebUiPasswordProtectionToggleChanged(bool isOn)
    {
        bool changed = WebUiPasswordProtection.SetProtectionEnabled(isOn);
        RefreshPasswordControls();

        if (changed)
        {
            SaveLoadSettings.SaveAndInvokeEvent();
        }
    }

    public void ShowPasswordTemporarily()
    {
        if (webUiPasswordText == null || webUiPasswordAstrisksText == null)
        {
            return;
        }

        string password = WebUiPasswordProtection.GetPasswordForUnityUi();
        bool hasPassword = !string.IsNullOrEmpty(password);

        if (!hasPassword)
        {
            return;
        }

        webUiPasswordText.text = password;
        webUiPasswordAstrisksText.gameObject.SetActive(false);

        Timer timer = new Timer(3000);
        timer.Elapsed += (sender, e) =>
        {
            timer.Stop();
            timer.Dispose();
            if (webUiPasswordText != null && webUiPasswordAstrisksText != null)
            {
                webUiPasswordText.text = string.Empty;
                webUiPasswordAstrisksText.gameObject.SetActive(true);
            }
        };
        timer.AutoReset = false;
        timer.Start();
    }

    public void ResetWebUiPassword()
    {
        WebUiPasswordProtection.ClearPassword();
        RefreshPasswordControls();
        SaveLoadSettings.SaveAndInvokeEvent();
    }

    public void SetFixtureName(string fixtureName)
    {
        SaveLoadSettings.SaveString(SaveLoadSettings.DeviceNetworkKey, fixtureName);
        SaveLoadSettings.SaveAndInvokeEvent();
        UpdateDeviceInfoDisplay();
    }

    public void UpdateDeviceInfoDisplay()
    {
        if (fixtureNameValueText != null)
        {
            fixtureNameValueText.text = SaveLoadSettings.LoadString(SaveLoadSettings.DeviceNetworkKey, "DMX Projector");
        }

        if (ipAddressValueText != null)
        {
            string networkType = "";
            if (NetworkingModeManager.Instance != null)
            {
                if (NetworkingModeManager.Instance.IsSAcnMode)
                {
                    networkType = "sAcn ";
                }
                else
                {
                    networkType = "Art-Net ";
                }
            }

            ipAddressValueText.text = networkType + ResolveLocalIpv4Address();
        }
    }

    private static string ResolveLocalIpv4Address()
    {
        try
        {
            IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());
            for (int i = 0; i < addresses.Length; i++)
            {
                IPAddress address = addresses[i];
                if (address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(address))
                {
                    return address.ToString();
                }
            }
        }
        catch (SocketException)
        {
        }

        return "Unavailable";
    }

    private void RefreshPasswordControls()
    {
        string password = WebUiPasswordProtection.GetPasswordForUnityUi();
        bool hasPassword = !string.IsNullOrEmpty(password);
        bool protectionEnabled = WebUiPasswordProtection.IsProtectionEnabled();

        if (webUiPasswordEnabledToggle != null)
        {
            webUiPasswordEnabledToggle.SetIsOnWithoutNotify(protectionEnabled);
        }

        if (webUiPasswordText != null)
        {
            webUiPasswordText.text = string.Empty;
        }

        if (webUiPasswordPlaceholderText != null)
        {
            webUiPasswordPlaceholderText.gameObject.SetActive(!hasPassword);
        }

        if (webUiPasswordAstrisksText != null)
        {
            webUiPasswordAstrisksText.gameObject.SetActive(hasPassword);
        }

        if (webUiPasswordResetButtonText != null)
        {
            webUiPasswordResetButtonText.color = hasPassword
                ? Color.white
                : new Color(0.75f, 0.75f, 0.75f, 1f);
        }

        if (passwordPanel != null)
        {
            passwordPanel.SetActive(protectionEnabled);
        }
    }

    private int GetMaxSelectableUniverse()
    {
        if (CapabilityService.Instance == null)
        {
            return 1;
        }

        string capabilityId = GetCapabilityId(universeLimitCapability);
        if (string.IsNullOrWhiteSpace(capabilityId))
        {
            return 1;
        }

        bool IsUnlimitedUniverseUnlocked = CapabilityService.Instance.ResolveBoolean(capabilityId);
        if (!IsUnlimitedUniverseUnlocked)
        {
            return 1;
        }

        if (NetworkingModeManager.Instance != null && NetworkingModeManager.Instance.IsSAcnMode)
        {
            return 63999;
        }
        else
        {
            return 32768;

        }

    }

    private void TriggerLockedCapability(CapabilityDefinition capabilityDefinition)
    {
        if (capabilityBlockUiTrigger == null)
        {
            return;
        }

        string capabilityId = GetCapabilityId(capabilityDefinition);
        if (string.IsNullOrWhiteSpace(capabilityId))
        {
            return;
        }

        capabilityBlockUiTrigger.NotifyBlocked(capabilityId);
    }

    private void SyncReceiverSubscription()
    {
        INetworkReceiver activeReceiver = NetworkingModeManager.Instance?.NetworkReceiver;
        if (ReferenceEquals(activeReceiver, _subscribedReceiver))
        {
            return;
        }

        if (_subscribedReceiver != null)
        {
            _subscribedReceiver.NoDataReceivedRecently -= ShowNetworkWarning;
            _subscribedReceiver.DataReceivedAgain -= HideNetworkWarning;
        }

        _subscribedReceiver = activeReceiver;

        if (_subscribedReceiver != null)
        {
            _subscribedReceiver.NoDataReceivedRecently += ShowNetworkWarning;
            _subscribedReceiver.DataReceivedAgain += HideNetworkWarning;
        }
    }

    private static string GetCapabilityId(CapabilityDefinition definition)
    {
        return definition != null ? definition.Id : null;
    }
}

public interface IShaderGlobalIntSetter
{
    void SetGlobalInt(string propertyName, int value);
}

public class UnityShaderGlobalIntSetter : IShaderGlobalIntSetter
{
    public void SetGlobalInt(string propertyName, int value)
    {
        Shader.SetGlobalInt(propertyName, value);
    }
}
