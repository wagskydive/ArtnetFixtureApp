using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IapPurchasePanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private CapabilityDatabase capabilityDatabase;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private IapPurchasePanelItem itemPrefab;

    private readonly List<IapPurchasePanelItem> _spawnedItems = new List<IapPurchasePanelItem>();

    public void Show()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

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
        if (definition == null || CapabilityService.Instance == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(definition.ProductId))
        {
            CapabilityService.Instance.UnlockProduct(definition.ProductId);
        }

        RebuildItems();
    }

    public bool IsUnlocked(CapabilityDefinition definition)
    {
        return definition != null
               && CapabilityService.Instance != null
               && definition.IsUnlockedBy(CapabilityService.Instance.Entitlements);
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
}
