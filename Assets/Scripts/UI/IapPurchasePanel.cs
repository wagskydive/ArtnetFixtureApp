using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IapPurchasePanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private CapabilityDatabase capabilityDatabase;
    [SerializeField] private UnityIapPurchaseGateway purchaseGateway;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private IapPurchasePanelItem itemPrefab;
    [SerializeField] private Popup NotAvailablePopup;

    private readonly List<IapPurchasePanelItem> _spawnedItems = new List<IapPurchasePanelItem>();

    private void Awake()
    {
        if (purchaseGateway == null)
        {
            purchaseGateway = FindFirstObjectByType<UnityIapPurchaseGateway>();
        }
    }

    private void OnEnable()
    {
        if (CapabilityService.Instance != null)
        {
            CapabilityService.Instance.EntitlementsChanged += HandleEntitlementsChanged;
        }
        Show();
    }

    private void OnDisable()
    {
        if (CapabilityService.Instance != null)
        {
            CapabilityService.Instance.EntitlementsChanged -= HandleEntitlementsChanged;
        }
        Hide();
    }

    public void Show()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        purchaseGateway?.SyncOwnedPurchases();
        RebuildItems();
    }

    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    public void RebuildItems()
    {
        ClearItems();

        CapabilityDatabase database = capabilityDatabase != null ? capabilityDatabase : CapabilityDatabase.Instance;
        if (database == null || contentRoot == null || itemPrefab == null)
        {
            return;
        }

        IReadOnlyList<CapabilityDefinition> definitions = database.CapabilityDefinitions;
        for (int i = 0; i < definitions.Count; i++)
        {
            CapabilityDefinition definition = definitions[i];
            if (definition == null)
            {
                continue;
            }

            IapPurchasePanelItem row = Instantiate(itemPrefab, contentRoot);
            row.Bind(definition, this);
            _spawnedItems.Add(row);
        }
    }

    public void Purchase(CapabilityDefinition definition)
    {
        if (definition == null)
        {
            return;
        }

        if (purchaseGateway == null)
        {
            purchaseGateway = FindFirstObjectByType<UnityIapPurchaseGateway>();
        }

        if (purchaseGateway != null && purchaseGateway.PurchaseProduct(definition.ProductId))
        {
            return;
        }

#if !UNITY_PURCHASING
        if (CapabilityService.Instance != null && !string.IsNullOrWhiteSpace(definition.ProductId))
        {
            // Fallback path for development/test environments without a configured store backend.
            CapabilityService.Instance.UnlockProduct(definition.ProductId);
        }
#endif

        RebuildItems();
    }

    public bool IsUnlocked(CapabilityDefinition definition)
    {
        if (definition != null && definition.IsConsumable)
        {
            return false;
        }

        return definition != null
               && CapabilityService.Instance != null
               && definition.IsUnlockedBy(CapabilityService.Instance.Entitlements);
    }

    public int GetConsumablePurchaseCount(CapabilityDefinition definition)
    {
        if (definition == null || !definition.IsConsumable || CapabilityService.Instance == null)
        {
            return 0;
        }

        return CapabilityService.Instance.GetConsumablePurchaseCount(definition.ProductId);
    }

    public string GetDisplayPrice(CapabilityDefinition definition)
    {
        if (definition == null)
        {
            return string.Empty;
        }

        if (purchaseGateway == null)
        {
            purchaseGateway = FindFirstObjectByType<UnityIapPurchaseGateway>();
        }

        return purchaseGateway != null ? purchaseGateway.GetDisplayPrice(definition) : string.Empty;
    }

    private void ClearItems()
    {
        for (int i = 0; i < _spawnedItems.Count; i++)
        {
            if (_spawnedItems[i] != null)
            {
                Destroy(_spawnedItems[i].gameObject);
            }
        }

        _spawnedItems.Clear();
    }

    private void HandleEntitlementsChanged()
    {
        RebuildItems();
    }

    internal void ShowNotAvailablePopup()
    {
        if(NotAvailablePopup != null)
        {
            NotAvailablePopup.gameObject.SetActive(true);
        }
    }
}
