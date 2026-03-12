using UnityEngine;
using UnityEngine.UI;

public class UI_DmxSettings : MonoBehaviour
{
    [SerializeField] private Text channelValueText;
    [SerializeField] private Text universeValueText;
    [SerializeField] private InputField channelInputField;
    [SerializeField] private InputField universeInputField;
    [SerializeField] private int currentPatternType = 0; // Pattern type selector (0=Static, 1=Pulse, 2=ColorShift)
    [SerializeField] private ArtNetReceiver artNetReceiver;
    [SerializeField] private UI_FixtureMeshManager fixtureMeshManager;

    private int currentDmxChannel = 1;
    private int currentDmxUniverse = 1;

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

    private void Start()
    {
        LoadPreferences();
        ApplySettingsToReceiver();
    }

    private void OnDisable()
    {
        SavePreferences();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SavePreferences();
        }
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
        SyncValuesFromReceiver();
        SaveLoadSettings.SaveInt(SaveLoadSettings.DmxChannelKey, CurrentDmxChannel);
        SaveLoadSettings.SaveInt(SaveLoadSettings.DmxUniverseKey, CurrentDmxUniverse);
        SaveLoadSettings.SaveInt(SaveLoadSettings.DmxPatternKey, CurrentPatternType);
        SaveLoadSettings.Save();
    }

    public void LoadPreferences()
    {
        CurrentDmxChannel = SaveLoadSettings.LoadInt(SaveLoadSettings.DmxChannelKey, CurrentDmxChannel);
        CurrentDmxUniverse = SaveLoadSettings.LoadInt(SaveLoadSettings.DmxUniverseKey, CurrentDmxUniverse);
        CurrentPatternType = SaveLoadSettings.LoadInt(SaveLoadSettings.DmxPatternKey, CurrentPatternType);
        ApplySettingsToReceiver();
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
