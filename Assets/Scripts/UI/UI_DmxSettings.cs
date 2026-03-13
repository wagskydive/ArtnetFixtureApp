using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;

public class UI_DmxSettings : MonoBehaviour
{
    [SerializeField] private Text channelValueText;
    [SerializeField] private Text universeValueText;
    [SerializeField] private InputField channelInputField;
    [SerializeField] private InputField universeInputField;
    [SerializeField] private Text fixtureNameValueText;
    [SerializeField] private Text ipAddressValueText;
    [SerializeField] private GameObject passwordPanel;
    [SerializeField] private Toggle webUiPasswordEnabledToggle;
    [SerializeField] private InputField webUiPasswordInputField;
    [SerializeField] private Button webUiPasswordApplyButton;
    [SerializeField] private int currentPatternType = 0; // Pattern type selector (0=Static, 1=Pulse, 2=ColorShift)
    [SerializeField] private ArtNetReceiver artNetReceiver;
    [SerializeField] private UI_FixtureMeshManager fixtureMeshManager;

    private int currentDmxChannel = 1;
    private int currentDmxUniverse = 1;
    private bool hasLoadedPreferences;
    private bool isLoadingPreferences;
    private bool isApplicationQuitting;

    public IShaderGlobalIntSetter ShaderGlobalIntSetter { get; set; } = new UnityShaderGlobalIntSetter();

    public int CurrentDmxChannel
    {
        get => currentDmxChannel;
        set
        {
            if (value >= 1 && value <= 512)
            {
                currentDmxChannel = value;
                UpdateChannelDisplay();

                ApplySettingsToReceiver();
            }
        }
    }

    public int CurrentDmxUniverse
    {
        get => currentDmxUniverse;
        set
        {
            if (value >= 1 && value <= 16)
            {
                currentDmxUniverse = value;
                UpdateUniverseDisplay();

                ApplySettingsToReceiver();
            }
        }
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
        LoadPreferences();
        ApplySettingsToReceiver();
        SaveLoadSettings.OnSettingsSaved += LoadSettingsAndUpdateDisplay;
        RefreshPasswordControls();
    }

    private void OnDestroy()
    {
        SaveLoadSettings.OnSettingsSaved -= LoadSettingsAndUpdateDisplay;
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
        CurrentDmxChannel = Mathf.Min(512, CurrentDmxChannel + 1);
        SavePreferences();
    }

    public void DecreaseChannel()
    {
        CurrentDmxChannel = Mathf.Max(1, CurrentDmxChannel - 1);
        SavePreferences();
    }

    public void IncreaseUniverse()
    {
        CurrentDmxUniverse = Mathf.Min(16, CurrentDmxUniverse + 1);
        SavePreferences();
    }

    public void DecreaseUniverse()
    {
        CurrentDmxUniverse = Mathf.Max(1, CurrentDmxUniverse - 1);
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

        SyncValuesFromReceiver();
        SaveLoadSettings.SaveInt(SaveLoadSettings.DmxChannelKey, CurrentDmxChannel);
        SaveLoadSettings.SaveInt(SaveLoadSettings.DmxUniverseKey, CurrentDmxUniverse);
        SaveLoadSettings.SaveInt(SaveLoadSettings.DmxPatternKey, CurrentPatternType);
        SaveLoadSettings.Save();
    }

    void LoadSettingsAndUpdateDisplay()
    {
        LoadPreferences(false);
    }

    public void LoadPreferences()
    {
        LoadPreferences(true);
    }

    public void LoadPreferences(bool apply)
    {
        isLoadingPreferences = true;
        CurrentDmxChannel = SaveLoadSettings.LoadInt(SaveLoadSettings.DmxChannelKey, CurrentDmxChannel);
        CurrentDmxUniverse = SaveLoadSettings.LoadInt(SaveLoadSettings.DmxUniverseKey, CurrentDmxUniverse);
        CurrentPatternType = SaveLoadSettings.LoadInt(SaveLoadSettings.DmxPatternKey, CurrentPatternType);
        isLoadingPreferences = false;
        hasLoadedPreferences = true;
        if (apply)
        {
            ApplySettingsToReceiver();
        }

        UpdateDeviceInfoDisplay();
        RefreshPasswordControls();
    }

    private void ApplySettingsToReceiver()
    {
        if (artNetReceiver == null)
        {
            return;
        }

        artNetReceiver.SetStartChannelFromUserInput(CurrentDmxChannel);
        artNetReceiver.SetUniverseFromUserInput(CurrentDmxUniverse);

        if (fixtureMeshManager != null)
        {
            fixtureMeshManager.SyncFixtureAddresses();
        }
    }

    private void UpdateChannelDisplay()
    {
        if (channelValueText != null)
        {
            channelValueText.text = currentDmxChannel.ToString();
        }

        if (channelInputField != null)
        {
            channelInputField.text = currentDmxChannel.ToString();
            channelInputField.interactable = false;
        }
    }

    private void UpdateUniverseDisplay()
    {
        if (universeValueText != null)
        {
            universeValueText.text = currentDmxUniverse.ToString();
        }

    }

    private void SyncValuesFromReceiver()
    {
        if (artNetReceiver == null)
        {
            return;
        }

        currentDmxChannel = Mathf.Clamp(artNetReceiver.StartChannel, 1, 512);
        currentDmxUniverse = Mathf.Clamp(artNetReceiver.GetUniverseForUserInput(), 1, 16);
        UpdateChannelDisplay();
        UpdateUniverseDisplay();
    }


    private void UpdateDeviceInfoDisplay()
    {
        if (fixtureNameValueText != null)
        {
            fixtureNameValueText.text = SaveLoadSettings.LoadString(SaveLoadSettings.WebUiDeviceNameKey, "ArtnetFixture");
        }

        if (ipAddressValueText != null)
        {
            ipAddressValueText.text = ResolveLocalIpv4Address();
        }
    }

    private static string ResolveLocalIpv4Address()
    {
        try
        {
            string host = Dns.GetHostName();
            IPAddress[] addresses = Dns.GetHostAddresses(host);
            for (int i = 0; i < addresses.Length; i++)
            {
                IPAddress candidate = addresses[i];
                if (candidate.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(candidate))
                {
                    return candidate.ToString();
                }
            }
        }
        catch (System.Exception)
        {
            // Ignore network lookup issues and fallback to localhost.
        }

        return "127.0.0.1";
    }

    public void OnWebUiPasswordProtectionToggleChanged(bool enabled)
    {
        WebUiPasswordProtection.SetEnabled(enabled);
        RefreshPasswordControls();
    }

    public void ApplyWebUiPasswordFromInput()
    {
        if (webUiPasswordInputField == null)
        {
            return;
        }

        bool saved = WebUiPasswordProtection.SetPassword(webUiPasswordInputField.text);
        if (saved)
        {
            webUiPasswordInputField.text = string.Empty;
        }

        RefreshPasswordControls();
    }

    private void RefreshPasswordControls()
    {
        bool enabled = WebUiPasswordProtection.IsEnabled();
        bool configured = WebUiPasswordProtection.HasConfiguredPassword();
        if(passwordPanel != null)
        {
            passwordPanel.SetActive(enabled);
        }

        if (webUiPasswordEnabledToggle != null)
        {
            webUiPasswordEnabledToggle.SetIsOnWithoutNotify(enabled);
        }

        if (webUiPasswordInputField != null)
        {
            Text placeholderText = webUiPasswordInputField.placeholder as Text;
            if (placeholderText != null)
            {
                placeholderText.text = configured ? "Enter new password" : "Set password";
            }
        }

        if (webUiPasswordApplyButton != null)
        {
            webUiPasswordApplyButton.interactable = enabled;
        }
    }

    private void OnPatternTypeChanged()
    {
        ShaderGlobalIntSetter.SetGlobalInt("_PatternType", currentPatternType);
        SavePreferences();
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
