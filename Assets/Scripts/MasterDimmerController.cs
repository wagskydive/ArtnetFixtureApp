using UnityEngine;

public class MasterDimmerController : MonoBehaviour
{
    public float CurrentMasterNormalized { get; private set; } = 1f;

    void Update()
    {
        INetworkReceiver receiver = NetworkingModeManager.Instance?.NetworkReceiver;
        if (receiver == null || receiver.DmxBuffer == null)
        {
            return;
        }

        CurrentMasterNormalized = receiver.GetFixtureChannelValue(1) / 255f;
    }
}
