using UnityEngine;
using System;

public class NetworkingModeManager : MonoBehaviour
{
    private const string AdvancedNetworkingCapabilityId = "capability.advanced.networking";

    public const int ArtNetModeIndex = 0;
    public const int SAcnModeIndex = 1;

    public static NetworkingModeManager Instance { get; private set; }
    public static event Action OnManagerReady;

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

        Debug.Log($"[NetworkingModeManager] Awake. Saved network mode index={initialMode}.");

        if (initialMode != ArtNetModeIndex && !IsAdvancedNetworkingUnlocked())
        {
            purchaseValidationManager ??= FindFirstObjectByType<PurchaseValidationManager>();
            purchaseValidationManager?.TryValidatePurchases();

            if (!IsAdvancedNetworkingUnlocked())
            {
                initialMode = ArtNetModeIndex;
                Debug.Log("[NetworkingModeManager] Advanced networking locked. Falling back to Art-Net startup mode.");
            }
        }

        AddressSettings startupAddress = ResolveStartupAddressSettings();
        bool shouldPersistStartupSelection = false;
        SetModeFromIndex(initialMode, shouldPersistStartupSelection, startupAddress);
        OnManagerReady?.Invoke();
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
        SetModeFromIndex(modeIndex, true, ResolveStartupAddressSettings());
    }

    private void SetModeFromIndex(int modeIndex, bool persistSelectedMode, AddressSettings fallbackAddress)
    {
        int clampedMode = Mathf.Clamp(modeIndex, ArtNetModeIndex, SAcnModeIndex);

        AddressSettings activeAddress = ResolveAddressFromActiveReceiverOrFallback(fallbackAddress);

        RemoveCurrentReceiverComponent();

        INetworkReceiver nextReceiver = clampedMode == ArtNetModeIndex
            ? gameObject.AddComponent<ArtNetReceiver>()
            : gameObject.AddComponent<SAcnReceiver>();

        nextReceiver.DmxBuffer = _dmxBuffer;
        nextReceiver.ReceiveNetworkData = true;
        nextReceiver.SetUniverse(activeAddress.UniverseForInput);
        nextReceiver.SetStartChannel(activeAddress.StartChannel);
        nextReceiver.TimeoutSeconds = activeAddress.TimeoutSeconds;

        Debug.Log($"[NetworkingModeManager] Activating {(clampedMode == ArtNetModeIndex ? "Art-Net" : "sACN")} mode with universe={activeAddress.UniverseForInput}, startChannel={activeAddress.StartChannel}, timeoutSeconds={activeAddress.TimeoutSeconds:0.###}, persistMode={persistSelectedMode}.");

        nextReceiver.StartReceiver();

        NetworkReceiver = nextReceiver;
        ActiveModeIndex = clampedMode;

        if (persistSelectedMode)
        {
            SaveLoadSettings.SaveInt(SaveLoadSettings.NetworkModeKey, clampedMode);
            SaveLoadSettings.SaveAndInvokeEvent();
        }
    }

    private static AddressSettings ResolveStartupAddressSettings()
    {
        int savedUniverse = Mathf.Clamp(SaveLoadSettings.LoadInt(SaveLoadSettings.DmxUniverseKey, 1), 1, 63999);
        int savedStartChannel = Mathf.Clamp(SaveLoadSettings.LoadInt(SaveLoadSettings.DmxChannelKey, 1), 1, 512);
        float savedTimeoutSeconds = Mathf.Max(0.1f, SaveLoadSettings.LoadFloat(SaveLoadSettings.SAcnTimeoutSecondsKey, 2f));

        Debug.Log($"[NetworkingModeManager] Loaded startup address from prefs: universe={savedUniverse}, startChannel={savedStartChannel}, timeoutSeconds={savedTimeoutSeconds:0.###}.");
        return new AddressSettings(savedUniverse, savedStartChannel, savedTimeoutSeconds);
    }

    private AddressSettings ResolveAddressFromActiveReceiverOrFallback(AddressSettings fallback)
    {
        if (NetworkReceiver == null)
        {
            return fallback;
        }

        int universeForInput = Mathf.Clamp(NetworkReceiver.GetUniverseForUserInput(), 1, 63999);
        int startChannel = Mathf.Clamp(NetworkReceiver.StartChannel, 1, 512);
        float timeoutSeconds = Mathf.Max(0.1f, NetworkReceiver.TimeoutSeconds);
        return new AddressSettings(universeForInput, startChannel, timeoutSeconds);
    }

    private readonly struct AddressSettings
    {
        public int UniverseForInput { get; }
        public int StartChannel { get; }
        public float TimeoutSeconds { get; }

        public AddressSettings(int universeForInput, int startChannel, float timeoutSeconds)
        {
            UniverseForInput = universeForInput;
            StartChannel = startChannel;
            TimeoutSeconds = timeoutSeconds;
        }
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
#if UNITY_EDITOR && !UNITY_ANDROID
        return true;
#endif
        return CapabilityService.Instance != null
            && CapabilityService.Instance.ResolveBoolean(AdvancedNetworkingCapabilityId, false);

        
    }
}
