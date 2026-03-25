public class CapabilitySystem
{
    private readonly ICapabilityLookup _capabilityLookup;
    private readonly EntitlementStore _entitlementStore;

    public CapabilitySystem(ICapabilityLookup capabilityLookup, EntitlementStore entitlementStore)
    {
        _capabilityLookup = capabilityLookup;
        _entitlementStore = entitlementStore;
    }

    public bool ResolveBoolean(string capabilityId, bool lockedValue = false)
    {
        if (!TryGetUnlockedDefinition(capabilityId, out CapabilityDefinition definition))
        {
            return lockedValue;
        }

        return definition.ValueType == CapabilityValueType.Boolean ? definition.UnlockedBooleanValue : lockedValue;
    }

    public int ResolveNumeric(string capabilityId, int lockedValue = 0)
    {
        if (!TryGetUnlockedDefinition(capabilityId, out CapabilityDefinition definition))
        {
            return lockedValue;
        }

        return definition.ValueType == CapabilityValueType.Numeric ? definition.UnlockedNumericValue : lockedValue;
    }

    private bool TryGetUnlockedDefinition(string capabilityId, out CapabilityDefinition definition)
    {
        definition = null;

        if (_capabilityLookup == null || _entitlementStore == null)
        {
            return false;
        }

        if (!_capabilityLookup.TryGetCapability(capabilityId, out definition))
        {
            return false;
        }

        return definition.IsUnlockedBy(_entitlementStore);
    }
}
