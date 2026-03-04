using UnityEngine;
using UnityEngine.UI;

public class UI_DmxSettings : MonoBehaviour
{
    [SerializeField] private InputField channelInputField;
    [SerializeField] private InputField universeInputField;
    [SerializeField] private int currentPatternType = 0; // Pattern type selector (0=Static, 1=Pulse, 2=ColorShift)

    private int currentDmxChannel = 1;
    private int currentDmxUniverse = 1;

    internal IShaderGlobalIntSetter ShaderGlobalIntSetter = new UnityShaderGlobalIntSetter();

    public int CurrentDmxChannel
    {
        get => currentDmxChannel;
        set
        {
            if (value >= 1 && value <= 512)
            {
                currentDmxChannel = value;
                channelInputField.text = value.ToString();
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
                universeInputField.text = value.ToString();
            }
        }
    }

    // Pattern type selector (0=Static, 1=Pulse, 2=ColorShift)
    public int CurrentPatternType
    {
        get => currentPatternType;
        set {
                currentPatternType = value;
            OnPatternTypeChanged();
            }
        }
    public void OnChannelValueChanged(string newText)
    {
        if (int.TryParse(newText, out int input))
        {
            CurrentDmxChannel = input;
        }
    }

    public void OnUniverseValueChanged(string newText)
    {
        if (int.TryParse(newText, out int input))
        {
            CurrentDmxUniverse = input;
        }
    }

    [Header("Pattern Controls")]
    public enum PatternType
    {
        Static,
        Pulse,
        ColorShift
}

    private void OnPatternTypeChanged()
    {
        ShaderGlobalIntSetter.SetGlobalInt("_PatternType", currentPatternType);
    }
}

internal interface IShaderGlobalIntSetter
{
    void SetGlobalInt(string propertyName, int value);
}

internal class UnityShaderGlobalIntSetter : IShaderGlobalIntSetter
{
    public void SetGlobalInt(string propertyName, int value)
    {
        Shader.SetGlobalInt(propertyName, value);
    }
}
