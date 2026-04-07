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
    public void Awake_RevertsSavedSAcnModeToArtNet_WhenAdvancedNetworkingIsLocked()
    {
        PlayerPrefs.SetInt(SaveLoadSettings.NetworkModeKey, (int)NetworkingMode.SAcn);
        PlayerPrefs.Save();

        var manager = _managerGo.AddComponent<NetworkingModeManager>();

        manager.SendMessage("Awake");

        Assert.AreEqual(NetworkingMode.ArtNet, manager.ActiveMode);
        Assert.AreEqual((int)NetworkingMode.ArtNet, PlayerPrefs.GetInt(SaveLoadSettings.NetworkModeKey, -1));
    }

    [Test]
    public void SetMode_PersistsSelectedModeImmediately()
    {
        var manager = _managerGo.AddComponent<NetworkingModeManager>();

        manager.SetMode(NetworkingMode.SAcn);

        Assert.AreEqual((int)NetworkingMode.SAcn, PlayerPrefs.GetInt(SaveLoadSettings.NetworkModeKey, -1));
    }
}
