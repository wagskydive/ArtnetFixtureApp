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
    }

    void Start()
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
        DmxSettingsBus.OnChanged += HandleSettingSave;
    }

    void OnDisable()
    {
        DmxSettingsBus.OnChanged -= HandleSettingSave;
    }
    private void HandleSettingSave(DmxSettingsSnapshot snapshot)
    {
        ApplyMode(snapshot.IsSAcnMode);
    }

    private void ApplyMode(bool isSAcn)
    {
        ApplyMode(isSAcn ? 1 : 0);
    }


    private void ApplyMode(int modeIndex)
    {
        if (ActiveModeIndex == modeIndex)
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

        nextReceiver.DmxBuffer = _dmxBuffer;
        nextReceiver.ReceiveNetworkData = true;


        Debug.Log($"[NetworkingModeManager] Activating {(clampedMode == ArtNetModeIndex ? "Art-Net" : "sACN")} mode");
        nextReceiver.StartReceiver();

        NetworkReceiver = nextReceiver;
        ActiveModeIndex = clampedMode;

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
