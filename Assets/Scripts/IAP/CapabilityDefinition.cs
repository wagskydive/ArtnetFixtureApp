using UnityEngine;
using System.Collections.Generic;

public enum CapabilityValueType
{
    Boolean = 0,
    Numeric = 1
}

[CreateAssetMenu(fileName = "CapabilityDefinition", menuName = "ArtnetFixture/Capabilities/Capability Definition")]
public class CapabilityDefinition : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private CapabilityValueType valueType = CapabilityValueType.Boolean;
    [SerializeField] private bool unlockedBooleanValue = true;
    [SerializeField] private int unlockedNumericValue = 1;
    [SerializeField] private string productId;
    [SerializeField] private List<string> additionalProductIds = new List<string>();
    [SerializeField] private bool consumable;
    [SerializeField] private string displayTitle;
    [TextArea]
    [SerializeField] private string displayDescription;
    [Min(0f)]
    [SerializeField] private float editorTestPriceUsd = 0.99f;

    public string Id => id;
    public CapabilityValueType ValueType => valueType;
    public bool UnlockedBooleanValue => unlockedBooleanValue;
    public int UnlockedNumericValue => unlockedNumericValue;
    public string ProductId => productId;
    public IReadOnlyList<string> AdditionalProductIds => additionalProductIds;
    public bool IsConsumable => consumable;
    public string DisplayTitle => displayTitle;
    public string DisplayDescription => displayDescription;
    public float EditorTestPriceUsd => editorTestPriceUsd;

    public bool IsUnlockedBy(EntitlementStore entitlementStore)
    {
        if (entitlementStore == null)
        {
            return false;
        }

        if (consumable)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(productId) && entitlementStore.IsUnlocked(productId))
        {
            return true;
        }

        for (int i = 0; i < additionalProductIds.Count; i++)
        {
            string alternativeProductId = additionalProductIds[i];
            if (!string.IsNullOrWhiteSpace(alternativeProductId) && entitlementStore.IsUnlocked(alternativeProductId))
            {
                return true;
            }
        }

        return false;
    }
}
