using UnityEngine;

public class CapabilityBlockUiTrigger : MonoBehaviour
{
    [SerializeField] private LockedCapabilityPanel lockedCapabilityPanel;

    public void NotifyBlocked(string capabilityId)
    {
        if (lockedCapabilityPanel == null)
        {
            return;
        }

        CapabilityDefinition definition = null;
        if (CapabilityService.Instance != null)
        {
            CapabilityService.Instance.TryGetCapability(capabilityId, out definition);
        }

        lockedCapabilityPanel.Show(capabilityId, definition);
    }
}
