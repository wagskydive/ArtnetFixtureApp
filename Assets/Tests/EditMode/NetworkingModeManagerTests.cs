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

}
