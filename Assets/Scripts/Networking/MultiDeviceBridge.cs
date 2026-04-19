using System;
using UnityEngine;

/// <summary>
/// Central orchestrator for:
/// - IAP state
/// - Device join/leave events
/// - License evaluation
/// - Feature gating events
///
/// Fully event-driven (no Update loop).
/// </summary>
[DefaultExecutionOrder(-100)]
public class MultiDeviceBridge : MonoBehaviour
{
    public static MultiDeviceBridge Instance;

    [Header("References")]
    public MultiDeviceLicenseManager licenseManager;
    public NetworkHeartbeat heartbeat;

    [Header("IAP Product")]
    public CapabilityDefinition product;

    // =============================
    // EVENTS
    // =============================

    public event Action<string> OnDeviceJoined;
    public event Action<string> OnDeviceLeft;

    public event Action OnMultiDeviceStateIsBlocked;
    public event Action OnMultiDeviceStateIsUnblocked;

    // =============================
    // INTERNAL STATE
    // =============================

    private bool lastBlockedState = false;
    private bool lastKnownIAPState = false;

    // =============================
    // UNITY LIFECYCLE
    // =============================

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Subscribe to LicenseManager-driven events
        if (licenseManager != null)
        {
            licenseManager.OnDeviceJoinedInternal += HandleDeviceJoined;
            licenseManager.OnDeviceLeftInternal += HandleDeviceLeft;
        }

        RefreshIAPState();
        EvaluateBlockState(); // initial state
    }

    private void OnDestroy()
    {
        if (licenseManager != null)
        {
            licenseManager.OnDeviceJoinedInternal -= HandleDeviceJoined;
            licenseManager.OnDeviceLeftInternal -= HandleDeviceLeft;
        }
    }

    // =============================
    // DEVICE EVENTS (FROM LICENSE MANAGER)
    // =============================

    private void HandleDeviceJoined(string deviceId)
    {
        OnDeviceJoined?.Invoke(deviceId);

        EvaluateBlockState();
    }

    private void HandleDeviceLeft(string deviceId)
    {
        OnDeviceLeft?.Invoke(deviceId);

        EvaluateBlockState();
    }

    // =============================
    // IAP HANDLING
    // =============================

    /// <summary>
    /// Call this when IAP might have changed (purchase/restored)
    /// </summary>
    public void RefreshIAPState()
    {
        bool ownsIAP = CheckIAPOwnership();

        if (ownsIAP == lastKnownIAPState)
            return;

        lastKnownIAPState = ownsIAP;

        // Update License Manager (local device)
        if (licenseManager != null)
        {
            licenseManager.ReportLocalIAP(ownsIAP);
        }

        // Update Heartbeat (broadcast)
        if (heartbeat != null)
        {
            heartbeat.SetLocalIAPState(ownsIAP);
        }

        Debug.Log($"[MultiDevice] IAP ownership changed: {ownsIAP}");

        EvaluateBlockState();
    }

    /// <summary>
    /// Replace with your real IAP check
    /// </summary>
    private bool CheckIAPOwnership()
    {
        // Example:
        // return IAPManager.Instance.HasProduct(productId);

        return CapabilityService.Instance.Entitlements.IsUnlocked(product.ProductId);
    }

    // =============================
    // CORE LOGIC
    // =============================

    private void EvaluateBlockState()
    {
        if (licenseManager == null)
            return;

        bool isBlocked = licenseManager.NeedsBlock();

        // Only fire on change
         if (isBlocked == lastBlockedState)
           return;

        lastBlockedState = isBlocked;

        if (isBlocked)
        {
            Debug.Log("[MultiDevice] BLOCKED");
            OnMultiDeviceStateIsBlocked?.Invoke();
        }
        else
        {
            Debug.Log("[MultiDevice] ALLOWED");
            OnMultiDeviceStateIsUnblocked?.Invoke();
        }
    }

    // =============================
    // PUBLIC API
    // =============================

    public bool IsMultiDeviceAllowed()
    {
        return licenseManager != null && licenseManager.IsMultiDeviceAllowed;
    }

    public bool ShouldBlockFeatures()
    {
        return licenseManager != null && licenseManager.NeedsBlock();
    }

    public bool IsCurrentlyBlocked()
    {
        return lastBlockedState;
    }
}