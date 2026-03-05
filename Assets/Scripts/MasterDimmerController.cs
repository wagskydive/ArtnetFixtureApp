using UnityEngine;

public class MasterDimmerController : MonoBehaviour
{
    [SerializeField] private ArtNetReceiver artNetReceiver;

    public float CurrentMasterNormalized { get; private set; } = 1f;

    void Update()
    {
        if (artNetReceiver == null || artNetReceiver.DmxBuffer == null)
        {
            return;
        }

        CurrentMasterNormalized = artNetReceiver.GetFixtureChannelValue(1) / 255f;
    }
}
