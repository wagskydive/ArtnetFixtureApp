using System.Collections.Generic;
using UnityEngine;

public class CapabilityDatabase : MonoBehaviour, ICapabilityLookup
{
    [SerializeField] private List<CapabilityDefinition> capabilityDefinitions = new List<CapabilityDefinition>();

    private readonly Dictionary<string, CapabilityDefinition> _definitionsById = new Dictionary<string, CapabilityDefinition>();

    public static CapabilityDatabase Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple CapabilityDatabase instances found. Keeping the first instance.", this);
            return;
        }

        Instance = this;
        RebuildLookup();
    }

    public bool TryGetCapability(string capabilityId, out CapabilityDefinition capabilityDefinition)
    {
        capabilityDefinition = null;

        if (string.IsNullOrWhiteSpace(capabilityId))
        {
            return false;
        }

        return _definitionsById.TryGetValue(capabilityId, out capabilityDefinition);
    }

    public void RebuildLookup()
    {
        _definitionsById.Clear();

        for (int i = 0; i < capabilityDefinitions.Count; i++)
        {
            CapabilityDefinition definition = capabilityDefinitions[i];
            if (definition == null)
            {
                Debug.LogError($"CapabilityDatabase contains a null definition at index {i}.", this);
                continue;
            }

            string id = definition.Id;
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogError($"CapabilityDatabase definition '{definition.name}' is missing an ID.", definition);
                continue;
            }

            if (_definitionsById.ContainsKey(id))
            {
                Debug.LogError($"Duplicate capability ID '{id}' detected in CapabilityDatabase.", definition);
                continue;
            }

            _definitionsById.Add(id, definition);
        }
    }
}
