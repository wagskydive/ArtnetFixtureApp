using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class UI_FixtureMeshManagerTests
{
    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey("dmx.fixture.count");
    }

    [Test]
    public void RebuildFixtures_AssignsSequentialStartChannelsInBlocksOf16()
    {
        var (manager, primaryReceiver, template) = CreateManagerWithTemplate();

        primaryReceiver.SetStartChannelFromUserInput(1);
        manager.RebuildFixtures(3);

        var receivers = Object.FindObjectsByType<ArtNetReceiver>(FindObjectsSortMode.None);
        Assert.That(receivers.Length, Is.EqualTo(3));

        AssertReceiverHasChannel(receivers, template, 1);
        AssertReceiverHasChannel(receivers, "FixtureTemplate_2", 17);
        AssertReceiverHasChannel(receivers, "FixtureTemplate_3", 33);

        Object.DestroyImmediate(manager.gameObject);
        Object.DestroyImmediate(template);
    }

    [Test]
    public void RebuildFixtures_ClampsBetweenOneAndSixteen()
    {
        var (manager, _, template) = CreateManagerWithTemplate();

        manager.RebuildFixtures(0);
        Assert.That(manager.FixtureCount, Is.EqualTo(1));

        manager.RebuildFixtures(32);
        Assert.That(manager.FixtureCount, Is.EqualTo(16));

        Object.DestroyImmediate(manager.gameObject);
        Object.DestroyImmediate(template);
    }

    [Test]
    public void RebuildFixtures_SharesDmxBufferAndDisablesNetworkOnSpawnedReceivers()
    {
        var (manager, primaryReceiver, template) = CreateManagerWithTemplate();

        primaryReceiver.DmxBuffer = new DmxBuffer();
        manager.RebuildFixtures(2);

        var receivers = Object.FindObjectsByType<ArtNetReceiver>(FindObjectsSortMode.None);
        ArtNetReceiver cloned = FindReceiverByName(receivers, "FixtureTemplate_2");

        Assert.That(cloned, Is.Not.Null);
        Assert.That(cloned.ReceiveNetworkData, Is.False);
        Assert.That(cloned.DmxBuffer, Is.SameAs(primaryReceiver.DmxBuffer));

        Object.DestroyImmediate(manager.gameObject);
        Object.DestroyImmediate(template);
    }

    [Test]
    public void Start_LoadsFixtureCountFromPreferences()
    {
        PlayerPrefs.SetInt("dmx.fixture.count", 4);

        var (manager, _, template) = CreateManagerWithTemplate();

        manager.SendMessage("Start");

        Assert.That(manager.FixtureCount, Is.EqualTo(4));

        Object.DestroyImmediate(manager.gameObject);
        Object.DestroyImmediate(template);
    }

    private static (UI_FixtureMeshManager manager, ArtNetReceiver primaryReceiver, GameObject template) CreateManagerWithTemplate()
    {
        var managerGo = new GameObject("fixture-manager");
        var manager = managerGo.AddComponent<UI_FixtureMeshManager>();

        var template = GameObject.CreatePrimitive(PrimitiveType.Quad);
        template.name = "FixtureTemplate";
        var receiver = template.AddComponent<ArtNetReceiver>();
        receiver.ReceiveNetworkData = false;
        receiver.DmxBuffer = new DmxBuffer();

        SetPrivateField(manager, "primaryReceiver", receiver);
        SetPrivateField(manager, "fixtureTemplate", template);

        return (manager, receiver, template);
    }

    private static void AssertReceiverHasChannel(ArtNetReceiver[] receivers, string name, int expectedStartChannel)
    {
        ArtNetReceiver receiver = FindReceiverByName(receivers, name);
        Assert.That(receiver, Is.Not.Null);
        Assert.That(receiver.StartChannel, Is.EqualTo(expectedStartChannel));
    }

    private static ArtNetReceiver FindReceiverByName(ArtNetReceiver[] receivers, string name)
    {
        foreach (var receiver in receivers)
        {
            if (receiver.gameObject.name == name)
            {
                return receiver;
            }
        }

        return null;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(target, value);
    }
}
