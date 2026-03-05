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

        WriteFixtureChannel(1, SliderToByte(masterDimmerSlider));
        WriteFixtureChannel(2, SliderToByte(redSlider));
        WriteFixtureChannel(3, SliderToByte(greenSlider));
        WriteFixtureChannel(4, SliderToByte(blueSlider));
        WriteFixtureChannel(5, SliderToByte(patternSlider));
        WriteFixtureChannel(6, SliderToByte(speedSlider));
        WriteFixtureChannel(7, SliderToByte(sizeSlider));
        WriteFixtureChannel(8, SliderToByte(strobeSlider));

        artNetReceiver.DmxBuffer.WriteFrame(_simulatedFrame, _simulatedFrame.Length);
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

        WriteFixtureChannel(channel, (byte)Mathf.RoundToInt(Mathf.Clamp01(normalizedValue) * 255f));

        if (artNetReceiver.DmxBuffer == null)
        {
            artNetReceiver.DmxBuffer = new DmxBuffer();
        }

        artNetReceiver.DmxBuffer.WriteFrame(_simulatedFrame, _simulatedFrame.Length);
        artNetReceiver.DmxBuffer.SwapIfNewFrame();
    }


    private void WriteFixtureChannel(int relativeChannel, byte value)
    {
        int absoluteChannel = artNetReceiver.StartChannel + relativeChannel - 1;
        if (absoluteChannel < 1 || absoluteChannel > _simulatedFrame.Length)
        {
            return;
        }

        _simulatedFrame[absoluteChannel - 1] = value;
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

    void Update()
    {
        PushFrameFromUi();
    }
}
