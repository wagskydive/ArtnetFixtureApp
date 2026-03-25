using System.Collections.Generic;
using UnityEngine;

#if UNITY_PURCHASING
using UnityEngine.Purchasing;
#endif

public class UnityIapPurchaseGateway : MonoBehaviour
#if UNITY_PURCHASING
    , IDetailedStoreListener
#endif
{
    [SerializeField] private CapabilityDatabase capabilityDatabase;

#if UNITY_PURCHASING
    private IStoreController _storeController;
#endif

    public bool IsReady
    {
        get
        {
#if UNITY_PURCHASING
            return _storeController != null;
#else
            return false;
#endif
        }
    }

    public void InitializePurchasing()
    {
#if UNITY_PURCHASING
        if (_storeController != null)
        {
            return;
        }

        CapabilityDatabase database = capabilityDatabase != null ? capabilityDatabase : CapabilityDatabase.Instance;
        if (database == null)
        {
            Debug.LogWarning("UnityIapPurchaseGateway could not initialize because no CapabilityDatabase is available.", this);
            return;
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        var productIds = new HashSet<string>();
        IReadOnlyList<CapabilityDefinition> definitions = database.CapabilityDefinitions;

        for (int i = 0; i < definitions.Count; i++)
        {
            CapabilityDefinition definition = definitions[i];
            if (definition == null)
            {
                continue;
            }

            AddProductId(definition.ProductId, productIds, builder);
            IReadOnlyList<string> additionalIds = definition.AdditionalProductIds;
            for (int additionalIndex = 0; additionalIndex < additionalIds.Count; additionalIndex++)
            {
                AddProductId(additionalIds[additionalIndex], productIds, builder);
            }
        }

        if (productIds.Count == 0)
        {
            Debug.LogWarning("UnityIapPurchaseGateway has no product IDs configured in CapabilityDefinition assets.", this);
            return;
        }

        UnityPurchasing.Initialize(this, builder);
#else
        Debug.LogWarning("Unity IAP package is not enabled. Define UNITY_PURCHASING and install Unity Purchasing to enable live purchases.", this);
#endif
    }

    public bool PurchaseProduct(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            return false;
        }

#if UNITY_PURCHASING
        if (_storeController == null)
        {
            InitializePurchasing();
        }

        if (_storeController == null)
        {
            return false;
        }

        _storeController.InitiatePurchase(productId);
        return true;
#else
        Debug.LogWarning($"Purchase requested for '{productId}', but Unity Purchasing is unavailable in this build.", this);
        return false;
#endif
    }

#if UNITY_PURCHASING
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        _storeController = controller;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"Unity IAP initialization failed: {error}", this);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"Unity IAP initialization failed: {error} ({message})", this);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        if (purchaseEvent?.purchasedProduct?.definition != null)
        {
            CapabilityService.Instance?.UnlockProduct(purchaseEvent.purchasedProduct.definition.id);
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        string productId = product != null && product.definition != null ? product.definition.id : "unknown";
        Debug.LogError($"Unity IAP purchase failed for '{productId}': {failureReason}", this);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        string productId = product != null && product.definition != null ? product.definition.id : "unknown";
        string reason = failureDescription != null ? failureDescription.reason.ToString() : "Unknown";
        string message = failureDescription != null ? failureDescription.message : string.Empty;
        Debug.LogError($"Unity IAP purchase failed for '{productId}': {reason} ({message})", this);
    }

    private static void AddProductId(string productId, HashSet<string> seenProductIds, ConfigurationBuilder builder)
    {
        if (string.IsNullOrWhiteSpace(productId) || !seenProductIds.Add(productId))
        {
            return;
        }

        builder.AddProduct(productId, ProductType.NonConsumable);
    }
#endif
}
