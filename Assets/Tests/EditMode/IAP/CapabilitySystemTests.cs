using System;
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
    public void EntitlementStore_MarkLocked_RemovesUnlockedProduct()
    {
        var store = new EntitlementStore();
        store.MarkUnlocked("product.fixture.limit");

        bool changed = store.MarkLocked("product.fixture.limit");

        Assert.That(changed, Is.True);
        Assert.That(store.IsUnlocked("product.fixture.limit"), Is.False);
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

        GameObject.DestroyImmediate(definition);
        GameObject.DestroyImmediate(databaseGo);
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

        GameObject.DestroyImmediate(definition);
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

        GameObject.DestroyImmediate(definition);
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
    public void EntitlementStore_RecordConsumablePurchase_PersistsAndIncrementsCount()
    {
        PlayerPrefs.DeleteKey(SaveLoadSettings.IapConsumablesKey);
        PlayerPrefs.Save();

        var writableStore = new EntitlementStore(persistLocally: true);
        int first = writableStore.RecordConsumablePurchase("product.coins.100");
        int second = writableStore.RecordConsumablePurchase("product.coins.100");

        Assert.That(first, Is.EqualTo(1));
        Assert.That(second, Is.EqualTo(2));
        Assert.That(writableStore.GetConsumablePurchaseCount("product.coins.100"), Is.EqualTo(2));

        var reloadedStore = new EntitlementStore(persistLocally: true);
        Assert.That(reloadedStore.GetConsumablePurchaseCount("product.coins.100"), Is.EqualTo(2));
    }

    [Test]
    public void CapabilityDefinition_IsUnlockedBy_ConsumableAlwaysReturnsFalse()
    {
        var definition = CreateCapabilityDefinition("capability.coins", CapabilityValueType.Numeric, "product.coins.100", false, 100);
        SetPrivateField(definition, "consumable", true);
        var store = new EntitlementStore();
        store.MarkUnlocked("product.coins.100");

        Assert.That(definition.IsUnlockedBy(store), Is.False);

        GameObject.DestroyImmediate(definition);
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

        GameObject.DestroyImmediate(universeLimit);
        GameObject.DestroyImmediate(outputBoost);
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

        GameObject.DestroyImmediate(universeLimit);
    }

    [Test]
    public void CapabilityService_SyncEntitlements_RevokesAndUnlocksProducts()
    {
        var serviceGo = new GameObject("capability-service");
        var service = serviceGo.AddComponent<CapabilityService>();
        InvokeAwake(service);

        service.UnlockProduct("product.keep");
        service.UnlockProduct("product.revoke");

        service.SyncEntitlements(new HashSet<string> { "product.keep", "product.new" });

        Assert.That(service.Entitlements.IsUnlocked("product.keep"), Is.True);
        Assert.That(service.Entitlements.IsUnlocked("product.new"), Is.True);
        Assert.That(service.Entitlements.IsUnlocked("product.revoke"), Is.False);

        GameObject.DestroyImmediate(serviceGo);
    }

    [Test]
    public void CapabilityService_SyncValidatedEntitlements_OnlyTouchesValidatedProducts()
    {
        var serviceGo = new GameObject("capability-service");
        var service = serviceGo.AddComponent<CapabilityService>();
        InvokeAwake(service);

        service.UnlockProduct("product.unlimited.universes");
        service.UnlockProduct("product.custom.gobos");

        service.SyncValidatedEntitlements(
            new HashSet<string> { "product.custom.gobos" },
            new HashSet<string> { "product.custom.gobos" });

        Assert.That(service.Entitlements.IsUnlocked("product.custom.gobos"), Is.True);
        Assert.That(service.Entitlements.IsUnlocked("product.unlimited.universes"), Is.True);

        GameObject.DestroyImmediate(serviceGo);
    }

    [Test]
    public void GooglePlayReceiptParser_ExtractPurchaseToken_ReturnsToken()
    {
        const string receipt = "{\"Store\":\"GooglePlay\",\"TransactionID\":\"txn-1\",\"Payload\":\"{\\\"json\\\":\\\"{\\\\\\\"purchaseToken\\\\\\\":\\\\\\\"token-123\\\\\\\",\\\\\\\"productId\\\\\\\":\\\\\\\"product.pro.bundle\\\\\\\"}\\\",\\\"signature\\\":\\\"sig\\\"}\"}";

        string token = GooglePlayReceiptParser.ExtractPurchaseToken(receipt);

        Assert.That(token, Is.EqualTo("token-123"));
    }

    [Test]
    public void GooglePlayReceiptParser_ExtractPurchaseToken_WithInvalidPayload_ReturnsNull()
    {
        string token = GooglePlayReceiptParser.ExtractPurchaseToken("not-json");
        Assert.That(token, Is.Null);
    }

    [Test]
    public void PurchaseValidationManager_HandleValidationResult_Revoked_AddsToRevokedSetOnly()
    {
        MethodInfo method = typeof(PurchaseValidationManager).GetMethod(
            "HandleValidationResult",
            BindingFlags.NonPublic | BindingFlags.Static);
        Type resultType = typeof(PurchaseValidationManager).GetNestedType(
            "ValidationResult",
            BindingFlags.NonPublic);
        object revoked = Enum.Parse(resultType, "Revoked");

        var validatedProducts = new HashSet<string>(StringComparer.Ordinal);
        var validProducts = new HashSet<string>(StringComparer.Ordinal);
        var revokedProducts = new HashSet<string>(StringComparer.Ordinal);
        method.Invoke(null, new object[] { "product.revoke", revoked, validatedProducts, validProducts, revokedProducts });

        Assert.That(validProducts.Contains("product.revoke"), Is.False);
        Assert.That(validatedProducts.Contains("product.revoke"), Is.True);
        Assert.That(revokedProducts.Contains("product.revoke"), Is.True);
    }

    [Test]
    public void PurchaseValidationManager_ShouldValidate_WhenNoPreviousTimestamp_ReturnsTrue()
    {
        const string validationKey = "iap_last_validation_unix";
        PlayerPrefs.DeleteKey(validationKey);
        PlayerPrefs.Save();

        var validationGo = new GameObject("purchase-validation");
        var validationManager = validationGo.AddComponent<PurchaseValidationManager>();

        bool shouldValidate = (bool)typeof(PurchaseValidationManager)
            .GetMethod("ShouldValidate", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(validationManager, null);

        Assert.That(shouldValidate, Is.True);

        GameObject.DestroyImmediate(validationGo);
    }

    [Test]
    public void PurchaseValidationManager_ShouldValidate_WhenWithinInterval_ReturnsFalse()
    {
        const string validationKey = "iap_last_validation_unix";
        long nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        SaveLoadSettings.SaveLong(validationKey, nowUnix);
        SaveLoadSettings.Save();

        var validationGo = new GameObject("purchase-validation");
        var validationManager = validationGo.AddComponent<PurchaseValidationManager>();

        bool shouldValidate = (bool)typeof(PurchaseValidationManager)
            .GetMethod("ShouldValidate", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(validationManager, null);

        Assert.That(shouldValidate, Is.False);

        GameObject.DestroyImmediate(validationGo);
    }

    [Test]
    public void PurchaseValidationManager_ShouldValidate_WhenPastInterval_ReturnsTrue()
    {
        const string validationKey = "iap_last_validation_unix";
        long staleUnix = DateTimeOffset.UtcNow.AddHours(-26).ToUnixTimeSeconds();
        SaveLoadSettings.SaveLong(validationKey, staleUnix);
        SaveLoadSettings.Save();

        var validationGo = new GameObject("purchase-validation");
        var validationManager = validationGo.AddComponent<PurchaseValidationManager>();

        bool shouldValidate = (bool)typeof(PurchaseValidationManager)
            .GetMethod("ShouldValidate", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(validationManager, null);

        Assert.That(shouldValidate, Is.True);

        GameObject.DestroyImmediate(validationGo);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("unknown")]
    [TestCase("UnsupportedIdentifier")]
    public void PurchaseValidationManager_NormalizeSystemDeviceId_InvalidValues_ReturnEmpty(string input)
    {
        string normalized = (string)typeof(PurchaseValidationManager)
            .GetMethod("NormalizeSystemDeviceId", BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, new object[] { input });

        Assert.That(normalized, Is.EqualTo(string.Empty));
    }

    [Test]
    public void PurchaseValidationManager_NormalizeSystemDeviceId_ValidValue_ReturnTrimmed()
    {
        string normalized = (string)typeof(PurchaseValidationManager)
            .GetMethod("NormalizeSystemDeviceId", BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, new object[] { "  device-123  " });

        Assert.That(normalized, Is.EqualTo("device-123"));
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

    private static void InvokeAwake(MonoBehaviour behaviour)
    {
        behaviour.GetType().GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(behaviour, null);
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
