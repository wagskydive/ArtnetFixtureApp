using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class ArtNetReceiverTests
{
    [Test]
    public void IsArtDmxPacket_ReturnsTrueForValidHeader()
    {
        var go = new GameObject("receiver");
        var receiver = go.AddComponent<ArtNetReceiver>();

        var data = new byte[18];
        data[0] = (byte)'A';
        data[1] = (byte)'r';
        data[2] = (byte)'t';
        data[3] = (byte)'-';
        data[4] = (byte)'N';
        data[5] = (byte)'e';
        data[6] = (byte)'t';
        data[7] = 0x00;
        data[8] = 0x00;
        data[9] = 0x50;

        var method = typeof(ArtNetReceiver).GetMethod("IsArtDmxPacket", BindingFlags.NonPublic | BindingFlags.Instance);
        var isValid = (bool)method.Invoke(receiver, new object[] { data });

        Assert.That(isValid, Is.True);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void IsArtDmxPacket_ReturnsFalseForShortOrInvalidData()
    {
        var go = new GameObject("receiver");
        var receiver = go.AddComponent<ArtNetReceiver>();
        var method = typeof(ArtNetReceiver).GetMethod("IsArtDmxPacket", BindingFlags.NonPublic | BindingFlags.Instance);

        var shortData = new byte[10];
        var invalidHeader = new byte[18];
        invalidHeader[0] = 0xFF;

        Assert.That((bool)method.Invoke(receiver, new object[] { shortData }), Is.False);
        Assert.That((bool)method.Invoke(receiver, new object[] { invalidHeader }), Is.False);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void Update_SwapsNewFrameIntoFrontBuffer()
    {
        var go = new GameObject("receiver");
        var receiver = go.AddComponent<ArtNetReceiver>();
        receiver.DmxBuffer = new DmxBuffer();

        receiver.DmxBuffer.WriteFrame(new byte[] { 99 }, 1);
        go.SendMessage("Update");

        Assert.That(receiver.DmxBuffer.GetChannel1Based(1), Is.EqualTo(99));

        Object.DestroyImmediate(go);
    }
}
