using UnityEngine;

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
    [SerializeField] private string displayTitle;
    [TextArea]
    [SerializeField] private string displayDescription;

    public string Id => id;
    public CapabilityValueType ValueType => valueType;
    public bool UnlockedBooleanValue => unlockedBooleanValue;
    public int UnlockedNumericValue => unlockedNumericValue;
    public string ProductId => productId;
    public string DisplayTitle => displayTitle;
    public string DisplayDescription => displayDescription;
}
