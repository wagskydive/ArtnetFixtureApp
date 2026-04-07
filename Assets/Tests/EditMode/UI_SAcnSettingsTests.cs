using NUnit.Framework;
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
        var multicastText = CreateText("multicast");
        var unicastText = CreateText("unicast");
        var portText = CreateText("port");

        SetPrivateField(settings, "transportModeText", transportText);
        SetPrivateField(settings, "multicastAddressText", multicastText);
        SetPrivateField(settings, "unicastBindAddressText", unicastText);
        SetPrivateField(settings, "listenPortText", portText);

        _settingsGo.SetActive(true);

        settings.SetUnicastMode();
        settings.SetUnicastBindAddress("10.10.10.5");
        settings.SetMulticastAddress("239.255.0.50");
        settings.SetListenPort(5569);

        Assert.That(transportText.text, Is.EqualTo("Unicast"));
        Assert.That(unicastText.text, Is.EqualTo("10.10.10.5"));
        Assert.That(multicastText.text, Is.EqualTo("239.255.0.50"));
        Assert.That(portText.text, Is.EqualTo("5569"));
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
