using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles multi-device detection + IAP sharing + session unlock logic.
/// Designed to be network-agnostic (you plug in your own discovery).
/// </summary>
public class MultiDeviceLicenseManager : MonoBehaviour
{
    // =============================
    // CONFIG
    // =============================

    [Header("Timing")]
    [Tooltip("How long before a device is considered gone (seconds)")]
    public float deviceTimeout = 3f;

    [Tooltip("How long an IAP device must be visible before unlocking (seconds)")]
    public float iapStabilityTime = 2f;

    public event Action<string> OnDeviceJoinedInternal;
    public event Action<string> OnDeviceLeftInternal;


    // Leader tracking
    private string currentLeaderId;
    private float leaderLastChangedTime;

    [Tooltip("How long the leader must stay stable before we trust it")]
    public float leaderStabilityTime = 1.5f;

    // =============================
    // INTERNAL STATE
    // =============================

    private class DeviceInfo
    {
        public string deviceId;
        public bool hasIAP;
        public float lastSeenTime;
    }

    private Dictionary<string, DeviceInfo> devices = new Dictionary<string, DeviceInfo>();

    private string localDeviceId;
    private bool localHasIAP = false;

    // Session latch (IMPORTANT FEATURE)
    private bool sessionUnlocked = false;

    // Stability timer
    private float iapDetectedTime = -1f;


    private float cleanupInterval = 1f;
    private float nextCleanupTime;

    private float startupTime;
    public float startupGracePeriod = 2f;

    // =============================
    // PUBLIC API
    // =============================

    /// <summary>
    /// True if multi-device usage is allowed (use this to gate your effects)
    /// </summary>
    public bool IsMultiDeviceAllowed
    {
        get { return sessionUnlocked; }
    }

    /// <summary>
    /// Current number of active devices (including self)
    /// </summary>
    public int ActiveDeviceCount
    {
        get { return devices.Count; }
    }

    // =============================
    // UNITY LIFECYCLE
    // =============================

    private void Awake()
    {
        startupTime = Time.time;
        // Generate or fetch persistent device ID
        localDeviceId = SystemInfo.deviceUniqueIdentifier;

        // Register self immediately
        UpdateDevice(localDeviceId, localHasIAP);
    }



    private void Update()
    {
        if (Time.time >= nextCleanupTime)
        {
            CleanupDevices();
            nextCleanupTime = Time.time + cleanupInterval;
        }

        EvaluateLicenseState();
    }

    // =============================
    // IAP INTEGRATION
    // =============================

    /// <summary>
    /// Call this from your IAP system when purchase state is known
    /// </summary>
    public void ReportLocalIAP(bool hasIAP)
    {
        localHasIAP = hasIAP;

        // Update self entry
        UpdateDevice(localDeviceId, localHasIAP);
    }

    // =============================
    // NETWORK INTEGRATION
    // =============================

    /// <summary>
    /// Call this whenever you detect or receive a heartbeat from another device
    /// </summary>
    public void UpdateDevice(string deviceId, bool hasIAP)
    {
        if (string.IsNullOrEmpty(deviceId))
            return;

        bool isNew = !devices.ContainsKey(deviceId);

        if (isNew)
        {
            devices[deviceId] = new DeviceInfo();
            devices[deviceId].deviceId = deviceId;
            Debug.Log("[MultiDevice] New device found: " + deviceId + " Total amount of devices in list is: " + devices.Count());
            OnDeviceJoinedInternal?.Invoke(deviceId);
        }

        // Ignore self duplicates (optional safety)

        devices[deviceId].hasIAP = hasIAP;
        devices[deviceId].lastSeenTime = Time.time;
    }

    // =============================
    // CORE LOGIC
    // =============================

    private void EvaluateLicenseState()
    {
        bool anyDeviceHasIAP = devices.Values.Any(d => d.hasIAP);

        // Stability check (prevents flicker unlocks)
        if (anyDeviceHasIAP)
        {
            if (iapDetectedTime < 0f)
                iapDetectedTime = Time.time;

            if (Time.time - iapDetectedTime >= iapStabilityTime)
            {
                sessionUnlocked = true;
            }
        }
        else
        {
            iapDetectedTime = -1f;
        }

        // IMPORTANT:
        // We DO NOT reset sessionUnlocked if IAP disappears
        // → This is your "stay unlocked until restart" behavior
    }

    public void CleanupDevices()
    {
        float now = Time.time;

        List<string> stringsToRemove = new List<string>();

        foreach (var kvp in devices)
        {
            Debug.Log("[MultiDevice] Checking for timeout of: " + kvp.Key + " Last Seen Time: " + kvp.Value.lastSeenTime + " Current Time: " + now);
            if (now - kvp.Value.lastSeenTime > deviceTimeout)
            {
                stringsToRemove.Add(kvp.Key);
            }
        }
        /*
        var toRemove = devices
            .Where(kvp => now - kvp.Value.lastSeenTime > deviceTimeout)
            .Select(kvp => kvp.Key)
            .ToList();
        */
        foreach (var key in stringsToRemove)
        {
            devices.Remove(key);
            // 🔥 Notify via bridge
            OnDeviceLeftInternal?.Invoke(key);
        }
    }

    // =============================
    // OPTIONAL HELPERS
    // =============================

    /// <summary>
    /// Should you block features right now?
    /// </summary>
    public bool NeedsBlock()
    {
        // If IAP unlocked → never block
        if (sessionUnlocked)
            return false;
        // ✅ Grace period: never block immediately after launch
        if (Time.time - startupTime < startupGracePeriod)
            return false;

        // If only one device → allow
        if (ActiveDeviceCount <= 1)
            return false;

        // Only block if NOT the leader
        return !IsLocalDeviceLeaderStable();
    }

    /// <summary>
    /// Debug info (useful for UI or logs)
    /// </summary>
    public string GetDebugInfo()
    {
        return $"Devices: {ActiveDeviceCount} | " +
               $"SessionUnlocked: {sessionUnlocked} | " +
               $"AnyIAP: {devices.Values.Any(d => d.hasIAP)}";
    }

    public void ForceRemoveDevice(string deviceId)
    {
        if (devices.Remove(deviceId))
        {
            OnDeviceLeftInternal?.Invoke(deviceId);
        }
    }

    private string GetCurrentLeaderId()
    {
        if (devices.Count == 0)
            return localDeviceId;

        // Optional: prioritize IAP devices
        var iapDevices = devices.Values
            .Where(d => d.hasIAP)
            .Select(d => d.deviceId)
            .OrderBy(id => id);

        if (iapDevices.Any())
            return iapDevices.First();

        // Otherwise pick lowest ID
        return devices.Keys.OrderBy(id => id).First();
    }

    public bool IsLocalDeviceLeaderStable()
    {
        string newLeader = GetCurrentLeaderId();

        if (newLeader != currentLeaderId)
        {
            currentLeaderId = newLeader;
            leaderLastChangedTime = Time.time;
        }

        bool isStable = (Time.time - leaderLastChangedTime) >= leaderStabilityTime;

        // ✅ If not stable → assume SAFE (do NOT block)
        if (!isStable)
            return true;

        return localDeviceId == currentLeaderId;
    }
}