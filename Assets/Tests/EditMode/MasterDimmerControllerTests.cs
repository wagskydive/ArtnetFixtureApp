using NUnit.Framework;
using UnityEngine;

public class MasterDimmerControllerTests
{
    [Test]
    public void Update_DoesNotThrow()
    {
        var go = new GameObject("dimmer");
        var controller = go.AddComponent<MasterDimmerController>();

        Assert.DoesNotThrow(() => controller.SendMessage("Update"));

        Object.DestroyImmediate(go);
    }
}
