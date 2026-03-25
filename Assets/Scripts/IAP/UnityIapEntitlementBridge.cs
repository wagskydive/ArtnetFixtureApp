using UnityEngine;

public class UnityIapEntitlementBridge : MonoBehaviour
{
    public void OnPurchaseSucceeded(string productId)
    {
        CapabilityService.Instance?.UnlockProduct(productId);
    }
}
