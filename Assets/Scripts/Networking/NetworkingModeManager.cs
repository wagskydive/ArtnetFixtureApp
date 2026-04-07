using UnityEngine;

public enum NetworkingMode
{
    ArtNet = 0,
    SAcn = 1
}

public class NetworkingModeManager : MonoBehaviour
{
    private const string AdvancedNetworkingCapabilityId = "capability.advanced.networking";

    public static NetworkingModeManager Instance { get; private set; }

    [SerializeField] private PurchaseValidationManager purchaseValidationManager;
    [SerializeField] private ArtNetReceiver artNetReceiver;
    [SerializeField] private SAcnReceiver sAcnReceiver;
    [SerializeField] private NetworkingMode startupMode = NetworkingMode.ArtNet;

    public NetworkingMode ActiveMode { get; private set; }
    public INetworkReceiver NetworkReceiver { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (artNetReceiver == null)
        {
            artNetReceiver = FindFirstObjectByType<ArtNetReceiver>();
        }

        if (sAcnReceiver == null)
        {
            sAcnReceiver = FindFirstObjectByType<SAcnReceiver>();
        }

        if (artNetReceiver != null && sAcnReceiver != null)
        {
            if (artNetReceiver.DmxBuffer == null)
            {
                artNetReceiver.DmxBuffer = new DmxBuffer();
            }

            sAcnReceiver.DmxBuffer = artNetReceiver.DmxBuffer;
            sAcnReceiver.SetUniverseFromUserInput(artNetReceiver.GetUniverseForUserInput());
            sAcnReceiver.SetStartChannelFromUserInput(artNetReceiver.StartChannel);
        }

        int savedMode = SaveLoadSettings.LoadInt(SaveLoadSettings.NetworkModeKey, (int)startupMode);
        NetworkingMode initialMode = (NetworkingMode)Mathf.Clamp(savedMode, 0, 1);

        if (initialMode != NetworkingMode.ArtNet && !IsAdvancedNetworkingUnlocked())
        {
            purchaseValidationManager ??= FindFirstObjectByType<PurchaseValidationManager>();
            purchaseValidationManager?.TryValidatePurchases();

            if (!IsAdvancedNetworkingUnlocked())
            {
                initialMode = NetworkingMode.ArtNet;
            }
        }

        SetMode(initialMode);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (artNetReceiver == null || sAcnReceiver == null)
        {
            return;
        }

        if (ActiveMode == NetworkingMode.SAcn)
        {
            sAcnReceiver.SetUniverseFromUserInput(artNetReceiver.GetUniverseForUserInput());
            sAcnReceiver.SetStartChannelFromUserInput(artNetReceiver.StartChannel);
        }
    }

    public void SetModeFromIndex(int modeIndex)
    {
        SetMode((NetworkingMode)Mathf.Clamp(modeIndex, 0, 1));
    }

    public void SetMode(NetworkingMode mode)
    {
        ActiveMode = mode;
        SaveLoadSettings.SaveInt(SaveLoadSettings.NetworkModeKey, (int)mode);
        SaveLoadSettings.Save();

        bool useArtNet = mode == NetworkingMode.ArtNet;
        SetReceiverState(artNetReceiver, useArtNet);
        SetReceiverState(sAcnReceiver, !useArtNet);
        NetworkReceiver = useArtNet ? artNetReceiver : sAcnReceiver;
    }

    private static bool IsAdvancedNetworkingUnlocked()
    {
        return CapabilityService.Instance != null
            && CapabilityService.Instance.ResolveBoolean(AdvancedNetworkingCapabilityId, false);
    }

    private static void SetReceiverState(INetworkReceiver receiver, bool isEnabled)
    {
        if (!(receiver is MonoBehaviour behaviour))
        {
            return;
        }

        if (isEnabled)
        {
            receiver.StartReceiver();
        }
        else
        {
            receiver.StopReceiver();
        }

        receiver.ReceiveNetworkData = isEnabled;
        behaviour.enabled = true;
    }
}
