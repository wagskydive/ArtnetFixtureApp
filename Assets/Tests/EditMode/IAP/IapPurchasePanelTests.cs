using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class IapPurchasePanelTests
{
    [Test]
    public void RebuildItems_ShowsLockedAndUnlockedState()
    {
        var databaseGo = new GameObject("capability-db");
        var database = databaseGo.AddComponent<CapabilityDatabase>();
        var capabilityA = CreateCapabilityDefinition("capability.universe.max", "Unlimited Universes", "product.pro.bundle");
        var capabilityB = CreateCapabilityDefinition("capability.output.boost", "Output Boost", "product.output.boost");
        SetPrivateField(capabilityA, "editorTestPriceUsd", 3.49f);
        SetPrivateField(capabilityB, "editorTestPriceUsd", 7.99f);
        SetPrivateField(database, "capabilityDefinitions", new List<CapabilityDefinition> { capabilityA, capabilityB });
        database.RebuildLookup();

        var serviceGo = new GameObject("capability-service");
        var service = serviceGo.AddComponent<CapabilityService>();
        SetPrivateField(service, "capabilityDatabase", database);
        InvokeAwake(service);
        service.UnlockProduct("product.pro.bundle");

        var panelGo = new GameObject("iap-panel");
        var panel = panelGo.AddComponent<IapPurchasePanel>();
        var root = new GameObject("root");
        var contentRoot = new GameObject("content").transform;
        var prefab = CreateItemPrefab();
        SetPrivateField(panel, "panelRoot", root);
        SetPrivateField(panel, "contentRoot", contentRoot);
        SetPrivateField(panel, "itemPrefab", prefab);
        SetPrivateField(panel, "capabilityDatabase", database);

        panel.Show();

        Assert.That(contentRoot.childCount, Is.EqualTo(2));
        var firstItem = contentRoot.GetChild(0).GetComponent<IapPurchasePanelItem>();
        var firstStatus = ((Text)GetPrivateField(firstItem, "statusText")).text;
        Assert.That(firstStatus, Is.EqualTo("Unlocked"));
        var firstPrice = ((Text)GetPrivateField(firstItem, "priceText")).text;
        Assert.That(firstPrice, Is.EqualTo("$3.49"));

        var secondButton = contentRoot.GetChild(1).GetComponentInChildren<Button>();
        Assert.That(secondButton.interactable, Is.True);
        var secondItem = contentRoot.GetChild(1).GetComponent<IapPurchasePanelItem>();
        var secondPrice = ((Text)GetPrivateField(secondItem, "priceText")).text;
        Assert.That(secondPrice, Is.EqualTo("$7.99"));

        Object.DestroyImmediate(panelGo);
        Object.DestroyImmediate(contentRoot.gameObject);
        Object.DestroyImmediate(root);
        Object.DestroyImmediate(prefab.gameObject);
        Object.DestroyImmediate(capabilityA);
        Object.DestroyImmediate(capabilityB);
        Object.DestroyImmediate(serviceGo);
        Object.DestroyImmediate(databaseGo);
    }

    [Test]
    public void RebuildItems_ConsumableCapability_ShowsPurchaseCountAndKeepsButtonEnabled()
    {
        var databaseGo = new GameObject("capability-db");
        var database = databaseGo.AddComponent<CapabilityDatabase>();
        var consumable = CreateCapabilityDefinition("capability.coins.pack", "Coin Pack", "product.coins.pack");
        SetPrivateField(consumable, "consumable", true);
        SetPrivateField(database, "capabilityDefinitions", new List<CapabilityDefinition> { consumable });
        database.RebuildLookup();

        var serviceGo = new GameObject("capability-service");
        var service = serviceGo.AddComponent<CapabilityService>();
        SetPrivateField(service, "capabilityDatabase", database);
        InvokeAwake(service);
        service.RecordConsumablePurchase("product.coins.pack");
        service.RecordConsumablePurchase("product.coins.pack");

        var panelGo = new GameObject("iap-panel");
        var panel = panelGo.AddComponent<IapPurchasePanel>();
        var root = new GameObject("root");
        var contentRoot = new GameObject("content").transform;
        var prefab = CreateItemPrefab();
        SetPrivateField(panel, "panelRoot", root);
        SetPrivateField(panel, "contentRoot", contentRoot);
        SetPrivateField(panel, "itemPrefab", prefab);
        SetPrivateField(panel, "capabilityDatabase", database);

        panel.Show();

        var item = contentRoot.GetChild(0).GetComponent<IapPurchasePanelItem>();
        var status = ((Text)GetPrivateField(item, "statusText")).text;
        var button = (Button)GetPrivateField(item, "purchaseButton");

        Assert.That(status, Is.EqualTo("Purchased: 2"));
        Assert.That(button.interactable, Is.True);

        Object.DestroyImmediate(panelGo);
        Object.DestroyImmediate(contentRoot.gameObject);
        Object.DestroyImmediate(root);
        Object.DestroyImmediate(prefab.gameObject);
        Object.DestroyImmediate(consumable);
        Object.DestroyImmediate(serviceGo);
        Object.DestroyImmediate(databaseGo);
    }

    private static IapPurchasePanelItem CreateItemPrefab()
    {
        var itemGo = new GameObject("item-prefab");
        var item = itemGo.AddComponent<IapPurchasePanelItem>();
        var title = new GameObject("title").AddComponent<Text>();
        var status = new GameObject("status").AddComponent<Text>();
        var price = new GameObject("price").AddComponent<Text>();
        var button = new GameObject("purchase").AddComponent<Button>();
        button.gameObject.AddComponent<Image>();

        title.transform.SetParent(itemGo.transform);
        status.transform.SetParent(itemGo.transform);
        price.transform.SetParent(itemGo.transform);
        button.transform.SetParent(itemGo.transform);

        SetPrivateField(item, "titleText", title);
        SetPrivateField(item, "statusText", status);
        SetPrivateField(item, "priceText", price);
        SetPrivateField(item, "purchaseButton", button);

        return item;
    }

    private static CapabilityDefinition CreateCapabilityDefinition(string id, string title, string productId)
    {
        var definition = ScriptableObject.CreateInstance<CapabilityDefinition>();
        SetPrivateField(definition, "id", id);
        SetPrivateField(definition, "displayTitle", title);
        SetPrivateField(definition, "productId", productId);
        return definition;
    }

    private static void InvokeAwake(MonoBehaviour behaviour)
    {
        behaviour.GetType().GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(behaviour, null);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(target, value);
    }

    private static object GetPrivateField(object target, string fieldName)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        return field.GetValue(target);
    }
}
