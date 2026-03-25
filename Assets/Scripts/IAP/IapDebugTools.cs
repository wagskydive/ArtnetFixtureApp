#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;

public class IapDebugTools : MonoBehaviour
{
    [SerializeField] private string debugProductId;

    public void UnlockProduct(string productId)
    {
        CapabilityService.Instance?.UnlockProduct(productId);
    }

    public void UnlockConfiguredProduct()
    {
        if (string.IsNullOrWhiteSpace(debugProductId))
        {
            return;
        }

        UnlockProduct(debugProductId);
    }

    public void ResetAllEntitlements()
    {
        CapabilityService.Instance?.ResetEntitlements();
    }
}
#endif
