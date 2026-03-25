using UnityEngine;
using System;

public class CapabilityService : MonoBehaviour
{
    [SerializeField] private CapabilityDatabase capabilityDatabase;
    [SerializeField] private bool persistEntitlementsLocally = true;

    private EntitlementStore _entitlementStore;
    private CapabilitySystem _capabilitySystem;
    public event Action EntitlementsChanged;

    public static CapabilityService Instance { get; private set; }

    public EntitlementStore Entitlements => _entitlementStore;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple CapabilityService instances found. Keeping the first instance.", this);
            return;
        }

        Instance = this;

        if (capabilityDatabase == null)
        {
            capabilityDatabase = CapabilityDatabase.Instance;
        }

        _entitlementStore = new EntitlementStore(persistEntitlementsLocally);
        _capabilitySystem = new CapabilitySystem(capabilityDatabase, _entitlementStore);
    }

    public int ResolveNumeric(string capabilityId, int lockedValue = 0)
    {
        return _capabilitySystem != null ? _capabilitySystem.ResolveNumeric(capabilityId, lockedValue) : lockedValue;
    }

    public bool ResolveBoolean(string capabilityId, bool lockedValue = false)
    {
        return _capabilitySystem != null ? _capabilitySystem.ResolveBoolean(capabilityId, lockedValue) : lockedValue;
    }

    public bool TryGetCapability(string capabilityId, out CapabilityDefinition definition)
    {
        definition = null;
        return capabilityDatabase != null && capabilityDatabase.TryGetCapability(capabilityId, out definition);
    }

    public void UnlockProduct(string productId)
    {
        if (_entitlementStore == null)
        {
            return;
        }

        if (_entitlementStore.MarkUnlocked(productId))
        {
            EntitlementsChanged?.Invoke();
        }
    }

    public void ResetEntitlements()
    {
        if (_entitlementStore == null)
        {
            return;
        }

        _entitlementStore.ResetAll();
        EntitlementsChanged?.Invoke();
    }
}
