using UnityEngine;
using System;
using System.Collections.Generic;

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

    public int RecordConsumablePurchase(string productId)
    {
        if (_entitlementStore == null)
        {
            return 0;
        }

        int updatedCount = _entitlementStore.RecordConsumablePurchase(productId);
        if (updatedCount > 0)
        {
            EntitlementsChanged?.Invoke();
        }

        return updatedCount;
    }

    public bool TryConsumeProduct(string productId, int amount = 1)
    {
        if (_entitlementStore == null)
        {
            return false;
        }

        bool changed = _entitlementStore.TryConsume(productId, amount);
        if (changed)
        {
            EntitlementsChanged?.Invoke();
        }

        return changed;
    }

    public int GetConsumablePurchaseCount(string productId)
    {
        if (_entitlementStore == null)
        {
            return 0;
        }

        return _entitlementStore.GetConsumablePurchaseCount(productId);
    }

    public void RevokeProduct(string productId)
    {
        if (_entitlementStore == null)
        {
            return;
        }

        if (_entitlementStore.MarkLocked(productId))
        {
            EntitlementsChanged?.Invoke();
        }
    }

    public void SyncEntitlements(IReadOnlyCollection<string> validProducts)
    {
        if (_entitlementStore == null)
        {
            return;
        }

        HashSet<string> validProductSet = validProducts != null
            ? new HashSet<string>(validProducts, StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);
        var currentProducts = new List<string>(_entitlementStore.GetUnlockedProductIds());
        bool changed = false;

        for (int i = 0; i < currentProducts.Count; i++)
        {
            string currentProductId = currentProducts[i];
            if (!validProductSet.Contains(currentProductId))
            {
                changed |= _entitlementStore.MarkLocked(currentProductId);
            }
        }

        foreach (string validProductId in validProductSet)
        {
            changed |= _entitlementStore.MarkUnlocked(validProductId);
        }

        if (changed)
        {
            EntitlementsChanged?.Invoke();
        }
    }

    public void SyncValidatedEntitlements(IReadOnlyCollection<string> validatedProducts, IReadOnlyCollection<string> validProducts)
    {
        if (_entitlementStore == null)
        {
            return;
        }

        var validatedProductSet = validatedProducts != null
            ? new HashSet<string>(validatedProducts, StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);
        var validProductSet = validProducts != null
            ? new HashSet<string>(validProducts, StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);

        bool changed = false;
        foreach (string validatedProductId in validatedProductSet)
        {
            if (!validProductSet.Contains(validatedProductId))
            {
                changed |= _entitlementStore.MarkLocked(validatedProductId);
            }
        }

        foreach (string validProductId in validProductSet)
        {
            changed |= _entitlementStore.MarkUnlocked(validProductId);
        }

        if (changed)
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

    public List<string> GetAllActiveProducts()
    {
        if (_entitlementStore == null)
        {
            return new List<string>();
        }

        return new List<string>(_entitlementStore.GetUnlockedProductIds());
    }
}
