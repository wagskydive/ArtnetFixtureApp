using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class PurchaseValidationManagerTests
{
    [Test]
    public void BuildRevocationMessage_SingleProduct_UsesSingularCopy()
    {
        var go = new GameObject("validation-manager");
        var manager = go.AddComponent<PurchaseValidationManager>();

        string message = InvokeBuildRevocationMessage(manager, new List<string> { "custom.gobos.upgrade" });

        Assert.That(message, Is.EqualTo("\"custom.gobos.upgrade\" was refunded and has been removed."));

        Object.DestroyImmediate(go);
    }

    [Test]
    public void BuildRevocationMessage_DedupesProductIds()
    {
        var go = new GameObject("validation-manager");
        var manager = go.AddComponent<PurchaseValidationManager>();

        string message = InvokeBuildRevocationMessage(manager, new List<string>
        {
            "custom.gobos.upgrade",
            "custom.gobos.upgrade",
            "premium.universes"
        });

        Assert.That(message, Is.EqualTo("2 purchases were refunded and have been removed: custom.gobos.upgrade, premium.universes."));

        Object.DestroyImmediate(go);
    }


    [Test]
    public void ShouldBypassServerValidationInEditor_DisabledFlag_ReturnsFalse()
    {
        var go = new GameObject("validation-manager");
        var manager = go.AddComponent<PurchaseValidationManager>();
        manager.debugForceValidInEditor = false;

        bool shouldBypass = (bool)typeof(PurchaseValidationManager)
            .GetMethod("ShouldBypassServerValidationInEditor", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(manager, null);

        Assert.That(shouldBypass, Is.False);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void ShouldBypassServerValidationInEditor_EnabledFlag_ReturnsTrueInEditor()
    {
        var go = new GameObject("validation-manager");
        var manager = go.AddComponent<PurchaseValidationManager>();
        manager.debugForceValidInEditor = true;

        bool shouldBypass = (bool)typeof(PurchaseValidationManager)
            .GetMethod("ShouldBypassServerValidationInEditor", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(manager, null);

        Assert.That(shouldBypass, Is.EqualTo(Application.isEditor));

        Object.DestroyImmediate(go);
    }

    private static string InvokeBuildRevocationMessage(PurchaseValidationManager manager, List<string> revokedProducts)
    {
        MethodInfo method = typeof(PurchaseValidationManager).GetMethod(
            "BuildRevocationMessage",
            BindingFlags.NonPublic | BindingFlags.Instance);

        return (string)method.Invoke(manager, new object[] { revokedProducts });
    }
}
