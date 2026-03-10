using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_FixtureMeshManager : MonoBehaviour
{
    private const string FixtureCountPrefKey = "dmx.fixture.count";

    [SerializeField] private ArtNetReceiver primaryReceiver;
    [SerializeField] private GameObject fixtureTemplate;
    [SerializeField] private Transform fixturesParent;
    [SerializeField] private Text fixtureCountValueText;
    [SerializeField][Range(1, 16)] private int minimumFixtures = 1;
    [SerializeField][Range(1, 16)] private int maximumFixtures = 16;

    private readonly List<GameObject> _spawnedFixtures = new List<GameObject>(16);

    public int FixtureCount => _spawnedFixtures.Count;

    private void Start()
    {
        RebuildFixtures(1, false);
        if (DmxModeManager.Instance.CurrentMode == DmxModeManager.FixtureMode.Standard)
        {
            int defaultCount = Mathf.Clamp(minimumFixtures, 1, maximumFixtures);
            int savedCount = PlayerPrefs.GetInt(FixtureCountPrefKey, defaultCount);
            int targetCount = Mathf.Clamp(savedCount, minimumFixtures, maximumFixtures);

            RebuildFixtures(targetCount);
        }

    }

    public void IncreaseFixtureCount()
    {
        RebuildFixtures(Mathf.Min(maximumFixtures, FixtureCount + 1));
    }

    public void DecreaseFixtureCount()
    {
        RebuildFixtures(Mathf.Max(minimumFixtures, FixtureCount - 1));
    }

    public void RebuildFixtures(int targetCount)
    {
        RebuildFixtures(targetCount, savePreference: true);
    }

    public void RebuildFixtures(int targetCount, bool savePreference)
    {
        int clampedCount = Mathf.Clamp(targetCount, minimumFixtures, maximumFixtures);

        if (fixtureTemplate == null)
        {
            UpdateFixtureCountDisplay(clampedCount);
            return;
        }

        EnsureFixtureListContainsTemplate();

        while (_spawnedFixtures.Count < clampedCount)
        {
            SpawnFixtureInstance();
        }

        while (_spawnedFixtures.Count > clampedCount)
        {
            RemoveLastFixtureInstance();
        }

        SyncFixtureAddresses();
        if (savePreference)
        {
            SaveFixtureCountPreference(clampedCount);
        }
        else
        {
            UpdateFixtureCountDisplay(clampedCount);
        }
    }

    public void RestoreSavedFixtureCount()
    {
        int defaultCount = Mathf.Clamp(minimumFixtures, 1, maximumFixtures);
        int savedCount = PlayerPrefs.GetInt(FixtureCountPrefKey, defaultCount);
        RebuildFixtures(savedCount, savePreference: false);
    }

    public void SyncFixtureAddresses()
    {
        if (primaryReceiver == null)
        {
            return;
        }

        int baseStartChannel = primaryReceiver.StartChannel;
        int baseUniverse0 = primaryReceiver.Universe;

        for (int i = 0; i < _spawnedFixtures.Count; i++)
        {
            ArtNetReceiver receiver = _spawnedFixtures[i].GetComponent<ArtNetReceiver>();
            if (receiver == null)
            {
                continue;
            }

            receiver.SetUniverseFromUserInput(baseUniverse0 + 1);
            receiver.SetStartChannelFromUserInput(baseStartChannel + (i * 16));
        }
    }

    private void EnsureFixtureListContainsTemplate()
    {
        if (_spawnedFixtures.Count == 0)
        {
            _spawnedFixtures.Add(fixtureTemplate);
        }
    }

    private void SpawnFixtureInstance()
    {
        Transform parent = fixturesParent != null ? fixturesParent : fixtureTemplate.transform.parent;
        GameObject instance = Instantiate(fixtureTemplate, parent);
        instance.name = $"{fixtureTemplate.name}_{_spawnedFixtures.Count + 1}";

        ArtNetReceiver templateReceiver = fixtureTemplate.GetComponent<ArtNetReceiver>();
        ArtNetReceiver instanceReceiver = instance.GetComponent<ArtNetReceiver>();

        if (templateReceiver != null && instanceReceiver != null)
        {
            instanceReceiver.ReceiveNetworkData = false;
            instanceReceiver.DmxBuffer = templateReceiver.DmxBuffer;
        }

        _spawnedFixtures.Add(instance);
    }

    private void RemoveLastFixtureInstance()
    {
        int lastIndex = _spawnedFixtures.Count - 1;
        GameObject target = _spawnedFixtures[lastIndex];
        _spawnedFixtures.RemoveAt(lastIndex);

        if (target != null && target != fixtureTemplate)
        {
            Destroy(target);
        }
    }

    private void SaveFixtureCountPreference(int count)
    {
        PlayerPrefs.SetInt(FixtureCountPrefKey, count);
        PlayerPrefs.Save();
        UpdateFixtureCountDisplay(count);
    }

    private void UpdateFixtureCountDisplay(int count)
    {
        if (fixtureCountValueText != null)
        {
            fixtureCountValueText.text = count.ToString();
        }
    }
}
