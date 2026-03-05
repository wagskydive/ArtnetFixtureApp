using UnityEngine;
using UnityEngine.UI;

public class UI_DmxEditorSimulator : MonoBehaviour
{
    [SerializeField] private ArtNetReceiver artNetReceiver;
    [SerializeField] private bool editorOnly = true;

    [Header("DMX Channel Sliders (1-8)")]
    [SerializeField] private Slider masterDimmerSlider;
    [SerializeField] private Slider redSlider;
    [SerializeField] private Slider greenSlider;
    [SerializeField] private Slider blueSlider;
    [SerializeField] private Slider patternSlider;
    [SerializeField] private Slider speedSlider;
    [SerializeField] private Slider sizeSlider;
    [SerializeField] private Slider strobeSlider;

    private readonly byte[] _simulatedFrame = new byte[512];

    public void PushFrameFromUi()
    {
        if (!CanSimulate())
        {
            return;
        }

        if (artNetReceiver.DmxBuffer == null)
        {
            artNetReceiver.DmxBuffer = new DmxBuffer();
        }

        _simulatedFrame[0] = SliderToByte(masterDimmerSlider);
        _simulatedFrame[1] = SliderToByte(redSlider);
        _simulatedFrame[2] = SliderToByte(greenSlider);
        _simulatedFrame[3] = SliderToByte(blueSlider);
        _simulatedFrame[4] = SliderToByte(patternSlider);
        _simulatedFrame[5] = SliderToByte(speedSlider);
        _simulatedFrame[6] = SliderToByte(sizeSlider);
        _simulatedFrame[7] = SliderToByte(strobeSlider);

        artNetReceiver.DmxBuffer.WriteFrame(_simulatedFrame, 8);
        artNetReceiver.DmxBuffer.SwapIfNewFrame();
    }

    public void SetChannelValue(int channel, float normalizedValue)
    {
        if (!CanSimulate())
        {
            return;
        }

        if (channel < 1 || channel > 8)
        {
            return;
        }

        _simulatedFrame[channel - 1] = (byte)Mathf.RoundToInt(Mathf.Clamp01(normalizedValue) * 255f);

        if (artNetReceiver.DmxBuffer == null)
        {
            artNetReceiver.DmxBuffer = new DmxBuffer();
        }

        artNetReceiver.DmxBuffer.WriteFrame(_simulatedFrame, 8);
        artNetReceiver.DmxBuffer.SwapIfNewFrame();
    }

    private bool CanSimulate()
    {
        if (artNetReceiver == null)
        {
            return false;
        }

        return !editorOnly || Application.isEditor;
    }

    private static byte SliderToByte(Slider slider)
    {
        if (slider == null)
        {
            return 0;
        }

        return (byte)Mathf.RoundToInt(Mathf.Clamp01(slider.normalizedValue) * 255f);
    }
}
