using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class CapabilitySystemTests
{
    [Test]
    public void EntitlementStore_MarkUnlocked_EnablesProductLookup()
    {
        var store = new EntitlementStore();

        Assert.That(store.IsUnlocked("product.fixture.limit"), Is.False);

        store.MarkUnlocked("product.fixture.limit");

        Assert.That(store.IsUnlocked("product.fixture.limit"), Is.True);
    }

    [Test]
    public void CapabilityDatabase_TryGetCapability_ReturnsDefinitionById()
    {
        var databaseGo = new GameObject("capability-db");
        var database = databaseGo.AddComponent<CapabilityDatabase>();
        var definition = CreateCapabilityDefinition("fixture.limit", CapabilityValueType.Numeric, "product.fixture.limit", false, 8);

        SetPrivateField(database, "capabilityDefinitions", new List<CapabilityDefinition> { definition });
        database.RebuildLookup();

        bool found = database.TryGetCapability("fixture.limit", out CapabilityDefinition resolved);

        Assert.That(found, Is.True);
        Assert.That(resolved, Is.EqualTo(definition));

        Object.DestroyImmediate(definition);
        Object.DestroyImmediate(databaseGo);
    }

    [Test]
    public void CapabilitySystem_ResolveNumeric_ReturnsLockedAndUnlockedValues()
    {
        var definition = CreateCapabilityDefinition("fixture.limit", CapabilityValueType.Numeric, "product.fixture.limit", false, 8);
        var lookup = new InMemoryCapabilityLookup(definition);
        var store = new EntitlementStore();
        var capabilitySystem = new CapabilitySystem(lookup, store);

        Assert.That(capabilitySystem.ResolveNumeric("fixture.limit", 3), Is.EqualTo(3));

        store.MarkUnlocked("product.fixture.limit");

        Assert.That(capabilitySystem.ResolveNumeric("fixture.limit", 3), Is.EqualTo(8));

        Object.DestroyImmediate(definition);
    }

    [Test]
    public void CapabilitySystem_ResolveBoolean_ReturnsLockedAndUnlockedValues()
    {
        var definition = CreateCapabilityDefinition("feature.advanced", CapabilityValueType.Boolean, "product.advanced", true, 0);
        var lookup = new InMemoryCapabilityLookup(definition);
        var store = new EntitlementStore();
        var capabilitySystem = new CapabilitySystem(lookup, store);

        Assert.That(capabilitySystem.ResolveBoolean("feature.advanced"), Is.False);

        store.MarkUnlocked("product.advanced");

        Assert.That(capabilitySystem.ResolveBoolean("feature.advanced"), Is.True);

        Object.DestroyImmediate(definition);
    }

    [Test]
    public void EntitlementStore_WithPersistence_RestoresUnlockedProducts()
    {
        PlayerPrefs.DeleteKey(SaveLoadSettings.IapEntitlementsKey);

        var writableStore = new EntitlementStore(persistLocally: true);
        writableStore.MarkUnlocked("product.bundle");
        writableStore.MarkUnlocked("product.extra");

        var reloadedStore = new EntitlementStore(persistLocally: true);

        Assert.That(reloadedStore.IsUnlocked("product.bundle"), Is.True);
        Assert.That(reloadedStore.IsUnlocked("product.extra"), Is.True);
    }


    [Test]
    public void EntitlementStore_WithPersistence_SavesEncryptedPayload()
    {
        PlayerPrefs.DeleteKey(SaveLoadSettings.IapEntitlementsKey);

        var store = new EntitlementStore(persistLocally: true);
        store.MarkUnlocked("product.bundle");

        string stored = PlayerPrefs.GetString(SaveLoadSettings.IapEntitlementsKey, string.Empty);
        Assert.That(stored, Does.StartWith("enc_v1:"));
        Assert.That(stored, Does.Not.Contain("product.bundle"));
    }

    [Test]
    public void EntitlementStore_WithLegacyPlaintext_MigratesToEncryptedStorage()
    {
        PlayerPrefs.SetString(SaveLoadSettings.IapEntitlementsKey, "product.bundle|product.extra");
        PlayerPrefs.Save();

        var store = new EntitlementStore(persistLocally: true);

        Assert.That(store.IsUnlocked("product.bundle"), Is.True);
        Assert.That(store.IsUnlocked("product.extra"), Is.True);

        string migrated = PlayerPrefs.GetString(SaveLoadSettings.IapEntitlementsKey, string.Empty);
        Assert.That(migrated, Does.StartWith("enc_v1:"));
        Assert.That(migrated, Does.Not.Contain("product.bundle"));
    }

    [Test]
    public void CapabilitySystem_SameProductUnlocksMultipleCapabilities()
    {
        var universeLimit = CreateCapabilityDefinition("capability.universe.max", CapabilityValueType.Numeric, "product.pro.bundle", false, 16);
        var outputBoost = CreateCapabilityDefinition("capability.output.boost", CapabilityValueType.Boolean, "product.pro.bundle", true, 0);

        var lookup = new InMemoryCapabilityLookup(universeLimit, outputBoost);
        var store = new EntitlementStore();
        var capabilitySystem = new CapabilitySystem(lookup, store);

        Assert.That(capabilitySystem.ResolveNumeric("capability.universe.max", 1), Is.EqualTo(1));
        Assert.That(capabilitySystem.ResolveBoolean("capability.output.boost", false), Is.False);

        store.MarkUnlocked("product.pro.bundle");

        Assert.That(capabilitySystem.ResolveNumeric("capability.universe.max", 1), Is.EqualTo(16));
        Assert.That(capabilitySystem.ResolveBoolean("capability.output.boost", false), Is.True);

        Object.DestroyImmediate(universeLimit);
        Object.DestroyImmediate(outputBoost);
    }

    [Test]
    public void CapabilitySystem_AlternateProductUnlock_UnlocksCapability()
    {
        var universeLimit = CreateCapabilityDefinition("capability.universe.max", CapabilityValueType.Numeric, "product.universe.single", false, 16);
        SetPrivateField(universeLimit, "additionalProductIds", new List<string> { "product.pro.bundle" });
        var lookup = new InMemoryCapabilityLookup(universeLimit);
        var store = new EntitlementStore();
        var capabilitySystem = new CapabilitySystem(lookup, store);

        Assert.That(capabilitySystem.ResolveNumeric("capability.universe.max", 1), Is.EqualTo(1));

        store.MarkUnlocked("product.pro.bundle");

        Assert.That(capabilitySystem.ResolveNumeric("capability.universe.max", 1), Is.EqualTo(16));

        Object.DestroyImmediate(universeLimit);
    }

    private static CapabilityDefinition CreateCapabilityDefinition(string id, CapabilityValueType valueType, string productId, bool unlockedBooleanValue, int unlockedNumericValue)
    {
        var definition = ScriptableObject.CreateInstance<CapabilityDefinition>();
        SetPrivateField(definition, "id", id);
        SetPrivateField(definition, "valueType", valueType);
        SetPrivateField(definition, "productId", productId);
        SetPrivateField(definition, "unlockedBooleanValue", unlockedBooleanValue);
        SetPrivateField(definition, "unlockedNumericValue", unlockedNumericValue);
        SetPrivateField(definition, "displayTitle", "Title");
        SetPrivateField(definition, "displayDescription", "Description");
        return definition;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(target, value);
    }

    private class InMemoryCapabilityLookup : ICapabilityLookup
    {
        private readonly Dictionary<string, CapabilityDefinition> _definitions;

        public InMemoryCapabilityLookup(params CapabilityDefinition[] definitions)
        {
            _definitions = new Dictionary<string, CapabilityDefinition>();
            for (int i = 0; i < definitions.Length; i++)
            {
                _definitions[definitions[i].Id] = definitions[i];
            }
        }

        public bool TryGetCapability(string capabilityId, out CapabilityDefinition capabilityDefinition)
        {
            return _definitions.TryGetValue(capabilityId, out capabilityDefinition);
        }
    }
}
