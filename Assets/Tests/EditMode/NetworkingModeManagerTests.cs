using NUnit.Framework;
using UnityEngine;

public class NetworkingModeManagerTests
{
    private GameObject _managerGo;

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey(SaveLoadSettings.NetworkModeKey);
        PlayerPrefs.DeleteAll();

        _managerGo = new GameObject("networking-mode-manager");
    }

    [TearDown]
    public void TearDown()
    {
        if (_managerGo != null)
        {
            Object.DestroyImmediate(_managerGo);
        }

        PlayerPrefs.DeleteAll();
    }

    [Test]
    public void Awake_RevertsSavedSAcnModeToArtNet_WhenAdvancedNetworkingIsLocked_WithoutOverwritingSavedPreference()
    {
        PlayerPrefs.SetInt(SaveLoadSettings.NetworkModeKey, NetworkingModeManager.SAcnModeIndex);
        PlayerPrefs.Save();

        var manager = _managerGo.AddComponent<NetworkingModeManager>();

        manager.SendMessage("Awake");

        Assert.AreEqual(NetworkingModeManager.ArtNetModeIndex, manager.ActiveModeIndex);
        Assert.AreEqual(NetworkingModeManager.SAcnModeIndex, PlayerPrefs.GetInt(SaveLoadSettings.NetworkModeKey, -1));
    }

    [Test]
    public void SetMode_PersistsSelectedModeImmediately()
    {
        var manager = _managerGo.AddComponent<NetworkingModeManager>();

        manager.SetModeFromIndex(NetworkingModeManager.SAcnModeIndex);

        Assert.AreEqual(NetworkingModeManager.SAcnModeIndex, PlayerPrefs.GetInt(SaveLoadSettings.NetworkModeKey, -1));
    }

    [Test]
    public void Awake_UsesSavedUniverseAndStartChannel_WhenCreatingInitialReceiver()
    {
        PlayerPrefs.SetInt(SaveLoadSettings.DmxUniverseKey, 42);
        PlayerPrefs.SetInt(SaveLoadSettings.DmxChannelKey, 123);
        PlayerPrefs.Save();

        var manager = _managerGo.AddComponent<NetworkingModeManager>();

        manager.SendMessage("Awake");

        Assert.That(manager.NetworkReceiver, Is.Not.Null);
        Assert.That(manager.NetworkReceiver.GetUniverseForUserInput(), Is.EqualTo(42));
        Assert.That(manager.NetworkReceiver.StartChannel, Is.EqualTo(123));
        Assert.That(PlayerPrefs.GetInt(SaveLoadSettings.DmxUniverseKey, -1), Is.EqualTo(42));
        Assert.That(PlayerPrefs.GetInt(SaveLoadSettings.DmxChannelKey, -1), Is.EqualTo(123));
    }
}
