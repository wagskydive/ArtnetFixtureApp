public interface ICapabilityLookup
{
    bool TryGetCapability(string capabilityId, out CapabilityDefinition capabilityDefinition);
}
