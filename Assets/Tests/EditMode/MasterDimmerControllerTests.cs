using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class MasterDimmerControllerTests
{
    [Test]
    public void Update_UsesDmxChannel1AsMasterDimmer()
    {
        var receiverGo = new GameObject("receiver");
        var receiver = receiverGo.AddComponent<ArtNetReceiver>();
        receiver.DmxBuffer = new DmxBuffer();
        receiver.DmxBuffer.WriteFrame(new byte[] { 64 }, 1);
        receiver.DmxBuffer.SwapIfNewFrame();

        var go = new GameObject("dimmer");
        var controller = go.AddComponent<MasterDimmerController>();
        typeof(MasterDimmerController)
            .GetField("artNetReceiver", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(controller, receiver);

        controller.SendMessage("Update");

        Assert.That(controller.CurrentMasterNormalized, Is.EqualTo(64f / 255f).Within(0.001f));

        Object.DestroyImmediate(receiverGo);
        Object.DestroyImmediate(go);
    }
}
