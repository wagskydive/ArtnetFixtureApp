using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SAcnSettingsTests
{
    private GameObject _managerGo;
    private GameObject _settingsGo;

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteAll();

        _managerGo = new GameObject("networking-mode-manager");
        var manager = _managerGo.AddComponent<NetworkingModeManager>();
        manager.SendMessage("Awake");
        manager.SetModeFromIndex(NetworkingModeManager.SAcnModeIndex);

        _settingsGo = new GameObject("sacn-settings");
        _settingsGo.SetActive(false);
        _settingsGo.AddComponent<UI_SAcnSettings>();
    }

    [TearDown]
    public void TearDown()
    {
        if (_settingsGo != null)
        {
            Object.DestroyImmediate(_settingsGo);
        }

        if (_managerGo != null)
        {
            Object.DestroyImmediate(_managerGo);
        }

        PlayerPrefs.DeleteAll();
    }

    [Test]
    public void OnEnable_DisablesPanel_WhenActiveReceiverIsNotSAcn()
    {
        var manager = _managerGo.GetComponent<NetworkingModeManager>();
        manager.SetModeFromIndex(NetworkingModeManager.ArtNetModeIndex);

        _settingsGo.SetActive(true);

        Assert.That(_settingsGo.activeSelf, Is.False);
    }

    [Test]
    public void TransportAndNetworkSetters_UpdateSAcnReceiver()
    {
        var manager = _managerGo.GetComponent<NetworkingModeManager>();
        var settings = _settingsGo.GetComponent<UI_SAcnSettings>();
        _settingsGo.SetActive(true);

        settings.SetUnicastMode();
        settings.SetUnicastBindAddress("192.168.1.50");
        settings.SetMulticastAddress("239.255.1.100");
        settings.SetListenPort(6000);

        var receiver = manager.NetworkReceiver as SAcnReceiver;

        Assert.That(receiver, Is.Not.Null);
        Assert.That(receiver.UseMulticast, Is.False);
        Assert.That(receiver.UnicastBindAddress, Is.EqualTo("192.168.1.50"));
        Assert.That(receiver.MulticastAddress, Is.EqualTo("239.255.1.100"));
        Assert.That(receiver.ListenPort, Is.EqualTo(6000));
    }

    [Test]
    public void InvalidAddresses_AreIgnored()
    {
        var manager = _managerGo.GetComponent<NetworkingModeManager>();
        var settings = _settingsGo.GetComponent<UI_SAcnSettings>();
        _settingsGo.SetActive(true);

        var receiver = manager.NetworkReceiver as SAcnReceiver;
        receiver.MulticastAddress = "239.255.0.1";
        receiver.UnicastBindAddress = "0.0.0.0";

        settings.SetMulticastAddress("192.168.0.1");
        settings.SetUnicastBindAddress("not-an-ip");

        Assert.That(receiver.MulticastAddress, Is.EqualTo("239.255.0.1"));
        Assert.That(receiver.UnicastBindAddress, Is.EqualTo("0.0.0.0"));
    }

    [Test]
    public void ListenPortButtons_ClampToValidRange()
    {
        var manager = _managerGo.GetComponent<NetworkingModeManager>();
        var settings = _settingsGo.GetComponent<UI_SAcnSettings>();
        _settingsGo.SetActive(true);

        settings.SetListenPort(1);
        settings.DecreaseListenPort();

        var receiver = manager.NetworkReceiver as SAcnReceiver;
        Assert.That(receiver.ListenPort, Is.EqualTo(1));

        settings.SetListenPort(65535);
        settings.IncreaseListenPort();

        Assert.That(receiver.ListenPort, Is.EqualTo(65535));
    }

    [Test]
    public void Setters_RefreshBoundTextLabels()
    {
        var settings = _settingsGo.GetComponent<UI_SAcnSettings>();

        var transportText = CreateText("transport");
        var portText = CreateText("port");

        SetPrivateField(settings, "transportModeText", transportText);
        SetPrivateField(settings, "listenPortText", portText);

        _settingsGo.SetActive(true);

        settings.SetUnicastMode();
        settings.SetUnicastBindAddress("10.10.10.5");
        settings.SetMulticastAddress("239.255.0.50");
        settings.SetListenPort(5569);

        Assert.That(transportText.text, Is.EqualTo("Unicast"));
        Assert.That(settings.UnicastBindAddress, Is.EqualTo("10.10.10.5"));
        Assert.That(settings.MulticastAddress, Is.EqualTo("239.255.0.50"));
        Assert.That(portText.text, Is.EqualTo("5569"));
    }

    [Test]
    public void ExtendedSetters_UpdateUniverseChannelMergeAndSubscriptions()
    {
        var manager = _managerGo.GetComponent<NetworkingModeManager>();
        var settings = _settingsGo.GetComponent<UI_SAcnSettings>();
        _settingsGo.SetActive(true);

        settings.SetUniverse(42);
        settings.SetStartChannel(128);
        settings.SetTimeoutSeconds(3.25f);
        settings.SetUseLtpMerge(true);
        settings.SetMulticastUniverseSubscriptionsCsv("2, 5, 5, invalid, 64001");

        var receiver = manager.NetworkReceiver as SAcnReceiver;
        Assert.That(receiver.GetUniverseForUserInput(), Is.EqualTo(42));
        Assert.That(receiver.StartChannel, Is.EqualTo(128));
        Assert.That(receiver.TimeoutSeconds, Is.EqualTo(3.25f).Within(0.01f));
        Assert.That(receiver.UseLtpMerge, Is.True);
        Assert.That(receiver.MulticastUniverseSubscriptions, Is.EqualTo(new List<int> { 1, 4, 63999 }));
    }

    [Test]
    public void ExtendedLabelRefresh_UpdatesAllAdditionalTexts()
    {
        var settings = _settingsGo.GetComponent<UI_SAcnSettings>();
        var universeText = CreateText("universe");
        var startChannelText = CreateText("start");
        var timeoutText = CreateText("timeout");
        var receiveText = CreateText("receive");
        var mergeText = CreateText("merge");
        var multicastUniversesText = CreateText("subs");

        SetPrivateField(settings, "universeText", universeText);
        SetPrivateField(settings, "startChannelText", startChannelText);
        SetPrivateField(settings, "timeoutSecondsText", timeoutText);
        SetPrivateField(settings, "receiveNetworkDataText", receiveText);
        SetPrivateField(settings, "mergeModeText", mergeText);
        SetPrivateField(settings, "multicastUniversesText", multicastUniversesText);

        _settingsGo.SetActive(true);
        settings.SetUniverse(10);
        settings.SetStartChannel(64);
        settings.SetTimeoutSeconds(1.5f);
        settings.SetUseLtpMerge(true);
        settings.SetMulticastUniverseSubscriptionsCsv("4,8");

        Assert.That(universeText.text, Is.EqualTo("10"));
        Assert.That(startChannelText.text, Is.EqualTo("64"));
        Assert.That(timeoutText.text, Is.EqualTo("1.5"));
        Assert.That(receiveText.text, Is.EqualTo("Disabled"));
        Assert.That(mergeText.text, Is.EqualTo("LTP"));
        Assert.That(multicastUniversesText.text, Is.EqualTo("4,8"));
    }

    [Test]
    public void UniverseAndMulticastAddress_StayInSyncUsingSacnConvention()
    {
        var manager = _managerGo.GetComponent<NetworkingModeManager>();
        var settings = _settingsGo.GetComponent<UI_SAcnSettings>();
        _settingsGo.SetActive(true);

        var receiver = manager.NetworkReceiver as SAcnReceiver;
        settings.SetUniverse(256);
        Assert.That(receiver.MulticastAddress, Is.EqualTo("239.255.1.0"));

        settings.SetMulticastAddress("239.255.0.1");
        Assert.That(receiver.GetUniverseForUserInput(), Is.EqualTo(1));
    }

    private static Text CreateText(string name)
    {
        var go = new GameObject(name);
        go.AddComponent<CanvasRenderer>();
        return go.AddComponent<Text>();
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        target.GetType()
            .GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(target, value);
    }
}
