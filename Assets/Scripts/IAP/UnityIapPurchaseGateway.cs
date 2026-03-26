using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_PURCHASING
using UnityEngine.Purchasing;
#endif

public class UnityIapPurchaseGateway : MonoBehaviour
#if UNITY_PURCHASING
    , IDetailedStoreListener
#endif
{
    public enum StoreBackendType
    {
        Unknown,
        GooglePlay,
        Fake,
        Other
    }

    [SerializeField] private CapabilityDatabase capabilityDatabase;
    private bool _hasAttemptedInitialization;
    private bool _initializationCompleted;
    private StoreBackendType _storeBackend = StoreBackendType.Unknown;
    private string _storeName = "Unknown";

#if UNITY_PURCHASING
    private IStoreController _storeController;
#endif

    public StoreBackendType ActiveStoreBackend => _storeBackend;
    public string ActiveStoreName => _storeName;
    public bool IsUsingRealStore => _storeBackend == StoreBackendType.GooglePlay;

    public bool IsReady
    {
        get
        {
#if UNITY_PURCHASING
            return _storeController != null && _initializationCompleted;
#else
            return false;
#endif
        }
    }

    private void Awake()
    {
        InitializePurchasing();
    }

    public void InitializePurchasing()
    {
#if UNITY_PURCHASING
        if (_hasAttemptedInitialization || _storeController != null)
        {
            return;
        }

        _hasAttemptedInitialization = true;

        CapabilityDatabase database = capabilityDatabase != null ? capabilityDatabase : CapabilityDatabase.Instance;
        if (database == null)
        {
            Debug.LogWarning("UnityIapPurchaseGateway could not initialize because no CapabilityDatabase is available.", this);
            return;
        }

        StandardPurchasingModule purchasingModule = StandardPurchasingModule.Instance();
        _storeName = purchasingModule.appStore.ToString();
        _storeBackend = ParseStoreBackend(_storeName);
        Debug.Log($"Initializing Unity IAP. Store: {_storeName}", this);

        var builder = ConfigurationBuilder.Instance(purchasingModule);
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
        if (!_initializationCompleted || _storeController == null || _storeController.products == null)
        {
            Debug.LogWarning($"Purchase requested for '{productId}' before Unity IAP finished initialization.", this);
            return false;
        }

        if (!IsUsingRealStore)
        {
            Debug.LogWarning("Purchase blocked: not connected to Google Play store.", this);
            return false;
        }

        Product product = _storeController.products.WithID(productId);
        if (product == null)
        {
            Debug.LogWarning($"Purchase requested for unknown product '{productId}'. Verify CapabilityDefinition product IDs.", this);
            return false;
        }

        if (!product.availableToPurchase)
        {
            Debug.LogWarning($"Purchase requested for unavailable product '{productId}'.", this);
            return false;
        }

        _storeController.InitiatePurchase(productId);
        return true;
#else
        Debug.LogWarning($"Purchase requested for '{productId}', but Unity Purchasing is unavailable in this build.", this);
        return false;
#endif
    }

    public string GetDisplayPrice(CapabilityDefinition definition)
    {
        if (definition == null)
        {
            return string.Empty;
        }

        string localizedPrice;
        if (TryGetLivePrice(definition.ProductId, out localizedPrice))
        {
            return localizedPrice;
        }

#if UNITY_EDITOR
        if (definition.EditorTestPriceUsd > 0f)
        {
            return string.Format(CultureInfo.InvariantCulture, "${0:0.00}", definition.EditorTestPriceUsd);
        }
#endif

        return string.Empty;
    }

    public bool TryGetLivePrice(string productId, out string localizedPrice)
    {
        localizedPrice = string.Empty;
        if (string.IsNullOrWhiteSpace(productId))
        {
            return false;
        }

#if UNITY_PURCHASING
        if (_storeController == null || _storeController.products == null)
        {
            Debug.LogWarning($"Price lookup failed for '{productId}': store not initialized.", this);
            return false;
        }

        Product product = _storeController.products.WithID(productId);
        if (product == null)
        {
            Debug.LogWarning($"Price lookup failed for '{productId}': product not found.", this);
            return false;
        }

        if (product.metadata == null || string.IsNullOrWhiteSpace(product.metadata.localizedPriceString))
        {
            Debug.LogWarning($"Price lookup failed for '{productId}': product metadata missing.", this);
            return false;
        }

        localizedPrice = product.metadata.localizedPriceString;
        return true;
#else
        return false;
#endif
    }

    public bool SyncOwnedPurchases()
    {
#if UNITY_PURCHASING
        if (_storeController == null || _storeController.products == null || CapabilityService.Instance == null)
        {
            return false;
        }

        bool changed = false;
        Product[] allProducts = _storeController.products.all;
        for (int i = 0; i < allProducts.Length; i++)
        {
            Product product = allProducts[i];
            if (product?.definition == null || product.definition.type != ProductType.NonConsumable)
            {
                continue;
            }

            if (product.hasReceipt)
            {
                int unlockedCountBefore = CapabilityService.Instance.Entitlements.GetUnlockedProductIds().Count;
                CapabilityService.Instance.UnlockProduct(product.definition.id);
                int unlockedCountAfter = CapabilityService.Instance.Entitlements.GetUnlockedProductIds().Count;
                if (unlockedCountAfter > unlockedCountBefore)
                {
                    changed = true;
                }
            }
        }

        return changed;
#else
        return false;
#endif
    }

#if UNITY_PURCHASING
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        _storeController = controller;
        _initializationCompleted = true;

        Debug.Log("IAP initialized", this);
        Debug.Log($"Store: {_storeName}", this);

        ProductCollection products = controller != null ? controller.products : null;
        if (products?.all == null)
        {
            return;
        }

        Product[] allProducts = products.all;
        for (int i = 0; i < allProducts.Length; i++)
        {
            Product product = allProducts[i];
            string productId = product?.definition != null ? product.definition.id : "unknown";
            bool availableToPurchase = product != null && product.availableToPurchase;
            Debug.Log($"Product: {productId} | availableToPurchase: {availableToPurchase}", this);
        }
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        _initializationCompleted = false;
        Debug.LogError($"Unity IAP initialization failed: {error}", this);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        _initializationCompleted = false;
        Debug.LogError($"Unity IAP initialization failed: {error} ({message})", this);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        if (!IsUsingRealStore)
        {
            if (_storeBackend == StoreBackendType.Fake)
            {
                Debug.LogWarning("Purchase ignored: FakeStore detected, unlock blocked", this);
            }
            else
            {
                Debug.LogWarning($"Purchase ignored: non-GooglePlay store '{_storeName}' detected, unlock blocked", this);
            }

            return PurchaseProcessingResult.Complete;
        }

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

    private static StoreBackendType ParseStoreBackend(string storeName)
    {
        if (string.IsNullOrWhiteSpace(storeName))
        {
            return StoreBackendType.Unknown;
        }

        string normalized = storeName.Trim().ToLowerInvariant();
        if (normalized.Contains("google"))
        {
            return StoreBackendType.GooglePlay;
        }

        if (normalized.Contains("fake"))
        {
            return StoreBackendType.Fake;
        }

        return StoreBackendType.Other;
    }
#endif
}
