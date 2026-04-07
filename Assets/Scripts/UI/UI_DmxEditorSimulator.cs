using UnityEngine;
using UnityEngine.UI;

public class UI_DmxEditorSimulator : MonoBehaviour
{
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
        INetworkReceiver receiver = NetworkingModeManager.Instance?.NetworkReceiver;
        if (!CanSimulate(receiver))
        {
            return;
        }

        if (receiver.DmxBuffer == null)
        {
            receiver.DmxBuffer = new DmxBuffer();
        }

        WriteFixtureChannel(receiver, 1, SliderToByte(masterDimmerSlider));
        WriteFixtureChannel(receiver, 2, SliderToByte(redSlider));
        WriteFixtureChannel(receiver, 3, SliderToByte(greenSlider));
        WriteFixtureChannel(receiver, 4, SliderToByte(blueSlider));
        WriteFixtureChannel(receiver, 5, SliderToByte(patternSlider));
        WriteFixtureChannel(receiver, 6, SliderToByte(speedSlider));
        WriteFixtureChannel(receiver, 7, SliderToByte(sizeSlider));
        WriteFixtureChannel(receiver, 8, SliderToByte(strobeSlider));

        receiver.DmxBuffer.WriteFrame(_simulatedFrame, _simulatedFrame.Length);
        receiver.DmxBuffer.SwapIfNewFrame();
    }

    public void SetChannelValue(int channel, float normalizedValue)
    {
        INetworkReceiver receiver = NetworkingModeManager.Instance?.NetworkReceiver;
        if (!CanSimulate(receiver))
        {
            return;
        }

        if (channel < 1 || channel > 8)
        {
            return;
        }

        WriteFixtureChannel(receiver, channel, (byte)Mathf.RoundToInt(Mathf.Clamp01(normalizedValue) * 255f));

        if (receiver.DmxBuffer == null)
        {
            receiver.DmxBuffer = new DmxBuffer();
        }

        receiver.DmxBuffer.WriteFrame(_simulatedFrame, _simulatedFrame.Length);
        receiver.DmxBuffer.SwapIfNewFrame();
    }

    private void WriteFixtureChannel(INetworkReceiver receiver, int relativeChannel, byte value)
    {
        int absoluteChannel = receiver.StartChannel + relativeChannel - 1;
        if (absoluteChannel < 1 || absoluteChannel > _simulatedFrame.Length)
        {
            return;
        }

        _simulatedFrame[absoluteChannel - 1] = value;
    }

    private bool CanSimulate(INetworkReceiver receiver)
    {
        if (receiver == null)
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
