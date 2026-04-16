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
    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DmxSettingsService.OnLoaded += HandleLoaded;
    }

    private void HandleLoaded(DmxSettingsSnapshot snapshot)
    {
        Initialize();
    }

    void Initialize()
    {
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

        ApplyMode(initialMode);
        isInitialized = true;
        OnManagerReady?.Invoke();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void OnEnable()
    {
        DmxSettingsBus.OnChanged += HandleSetting;

    }

    void OnDisable()
    {
        DmxSettingsBus.OnChanged -= HandleSetting;


    }
    private void HandleSetting(DmxSettingsSnapshot snapshot)
    {
        ApplyMode(snapshot.IsSAcnMode);
    }

    private void ApplyMode(bool isSAcn)
    {
        ApplyMode(isSAcn ? 1 : 0);
    }


    private void ApplyMode(int modeIndex)
    {
        if (ActiveModeIndex == modeIndex && isInitialized)
        {
            return;
        }
        int clampedMode = Mathf.Clamp(modeIndex, ArtNetModeIndex, SAcnModeIndex);

        if (NetworkReceiver != null)
        {
            INetworkReceiver lastReceiver = NetworkReceiver;
            lastReceiver.StopReceiver();

        }
        RemoveCurrentReceiverComponent();

        INetworkReceiver nextReceiver;
        if (clampedMode == ArtNetModeIndex)
        {
            nextReceiver = GetComponent<ArtNetReceiver>();
            if (nextReceiver == null)
            {
                nextReceiver = gameObject.AddComponent<ArtNetReceiver>();
            }
        }
        else
        {
            nextReceiver = GetComponent<SAcnReceiver>();
            if (nextReceiver == null)
            {
                nextReceiver = gameObject.AddComponent<SAcnReceiver>();
            }
        }

        nextReceiver.Buffer = _dmxBuffer;
        nextReceiver.ReceiveNetworkData = true;


        Debug.Log($"[NetworkingModeManager] Activating {(clampedMode == ArtNetModeIndex ? "Art-Net" : "sACN")} mode");

        ActivateReceiver(nextReceiver);

        NetworkReceiver = nextReceiver;
        ActiveModeIndex = clampedMode;

    }


    void ActivateReceiver(INetworkReceiver receiver)
    {
        // 1. Apply current settings FIRST
        if (receiver is IDmxSettingsConsumer consumer)
        {
            consumer.ApplyDmxSettings(DmxSettingsService.Instance.CurrentDmxSettings);
        }

        // 2. THEN start
        if (receiver.ReceiveNetworkData)
        {
            receiver.StartReceiver();
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
