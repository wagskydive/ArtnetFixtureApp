using UnityEngine;

public class NetworkingModeManager : MonoBehaviour
{
    private const string AdvancedNetworkingCapabilityId = "capability.advanced.networking";

    public const int ArtNetModeIndex = 0;
    public const int SAcnModeIndex = 1;

    public static NetworkingModeManager Instance { get; private set; }

    [SerializeField] private PurchaseValidationManager purchaseValidationManager;
    [SerializeField] private int startupModeIndex = ArtNetModeIndex;

    public int ActiveModeIndex { get; private set; }
    public bool IsSAcnMode => ActiveModeIndex == SAcnModeIndex;
    public INetworkReceiver NetworkReceiver { get; private set; }

    private DmxBuffer _dmxBuffer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _dmxBuffer = new DmxBuffer();

        int initialMode = Mathf.Clamp(
            SaveLoadSettings.LoadInt(SaveLoadSettings.NetworkModeKey, Mathf.Clamp(startupModeIndex, ArtNetModeIndex, SAcnModeIndex)),
            ArtNetModeIndex,
            SAcnModeIndex);

        if (initialMode != ArtNetModeIndex && !IsAdvancedNetworkingUnlocked())
        {
            purchaseValidationManager ??= FindFirstObjectByType<PurchaseValidationManager>();
            purchaseValidationManager?.TryValidatePurchases();

            if (!IsAdvancedNetworkingUnlocked())
            {
                initialMode = ArtNetModeIndex;
            }
        }

        SetModeFromIndex(initialMode);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void SetModeFromIndex(int modeIndex)
    {
        int clampedMode = Mathf.Clamp(modeIndex, ArtNetModeIndex, SAcnModeIndex);

        int currentUniverseForInput = NetworkReceiver?.GetUniverseForUserInput() ?? 1;
        int currentStartChannel = NetworkReceiver?.StartChannel ?? 1;
        float currentTimeoutSeconds = NetworkReceiver?.TimeoutSeconds ?? 2f;

        RemoveCurrentReceiverComponent();

        INetworkReceiver nextReceiver = clampedMode == ArtNetModeIndex
            ? gameObject.AddComponent<ArtNetReceiver>()
            : gameObject.AddComponent<SAcnReceiver>();

        nextReceiver.DmxBuffer = _dmxBuffer;
        nextReceiver.ReceiveNetworkData = true;
        nextReceiver.SetUniverseFromUserInput(currentUniverseForInput);
        nextReceiver.SetStartChannelFromUserInput(currentStartChannel);
        nextReceiver.TimeoutSeconds = currentTimeoutSeconds;

        nextReceiver.StartReceiver();

        NetworkReceiver = nextReceiver;
        ActiveModeIndex = clampedMode;

        SaveLoadSettings.SaveInt(SaveLoadSettings.NetworkModeKey, clampedMode);
        SaveLoadSettings.Save();
    }

    private void RemoveCurrentReceiverComponent()
    {
        if (NetworkReceiver == null)
        {
            return;
        }

        NetworkReceiver.ReceiveNetworkData = false;
        NetworkReceiver.StopReceiver();

        if (NetworkReceiver is MonoBehaviour receiverComponent)
        {
            if (Application.isPlaying)
            {
                Destroy(receiverComponent);
            }
            else
            {
                DestroyImmediate(receiverComponent);
            }
        }

        NetworkReceiver = null;
    }

    private static bool IsAdvancedNetworkingUnlocked()
    {
        return CapabilityService.Instance != null
            && CapabilityService.Instance.ResolveBoolean(AdvancedNetworkingCapabilityId, false);
    }
}
