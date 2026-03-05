using UnityEngine;
using UnityEngine.UI;

public class UI_DmxSettings : MonoBehaviour
{
    private const string ChannelPrefKey = "dmx.channel";
    private const string UniversePrefKey = "dmx.universe";
    private const string PatternPrefKey = "dmx.pattern";

    [SerializeField] private InputField channelInputField;
    [SerializeField] private InputField universeInputField;
    [SerializeField] private int currentPatternType = 0; // Pattern type selector (0=Static, 1=Pulse, 2=ColorShift)

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
                if (channelInputField != null)
                {
                    channelInputField.text = value.ToString();
                }
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
                if (universeInputField != null)
                {
                    universeInputField.text = value.ToString();
                }
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

    public void OnChannelValueChanged(string newText)
    {
        if (int.TryParse(newText, out int input))
        {
            CurrentDmxChannel = input;
            SavePreferences();
        }
    }

    public void OnUniverseValueChanged(string newText)
    {
        if (int.TryParse(newText, out int input))
        {
            CurrentDmxUniverse = input;
            SavePreferences();
        }
    }

    public enum PatternType
    {
        Static,
        Pulse,
        ColorShift
    }

    public void SavePreferences()
    {
        PlayerPrefs.SetInt(ChannelPrefKey, CurrentDmxChannel);
        PlayerPrefs.SetInt(UniversePrefKey, CurrentDmxUniverse);
        PlayerPrefs.SetInt(PatternPrefKey, CurrentPatternType);
        PlayerPrefs.Save();
    }

    public void LoadPreferences()
    {
        CurrentDmxChannel = PlayerPrefs.GetInt(ChannelPrefKey, CurrentDmxChannel);
        CurrentDmxUniverse = PlayerPrefs.GetInt(UniversePrefKey, CurrentDmxUniverse);
        CurrentPatternType = PlayerPrefs.GetInt(PatternPrefKey, CurrentPatternType);
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
