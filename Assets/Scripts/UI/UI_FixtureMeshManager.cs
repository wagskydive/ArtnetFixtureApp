using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_FixtureMeshManager : MonoBehaviour
{
    private INetworkReceiver primaryReceiver;
    [SerializeField] private GameObject fixtureTemplate;
    [SerializeField] private Transform fixturesParent;
    [SerializeField] private Text fixtureCountValueText;
    [SerializeField][Range(1, 16)] private int minimumFixtures = 1;
    [SerializeField][Range(1, 16)] private int maximumFixtures = 16;

    private readonly List<GameObject> _spawnedFixtures = new List<GameObject>(16);

    public int FixtureCount => _spawnedFixtures.Count;

    private void Start()
    {
        primaryReceiver = NetworkingModeManager.Instance.NetworkReceiver;
        RebuildFixtures();


    }

    void OnEnable()
    {
        SaveLoadSettings.OnFixtureCountSaved += RebuildFixtures;
    }
    void OnDisable()
    {
        SaveLoadSettings.OnFixtureCountSaved -= RebuildFixtures;
    }


    public void IncreaseFixtureCount()
    {
        SaveLoadSettings.SaveFixtureCount(Mathf.Min(maximumFixtures, FixtureCount + 1));

    }

    public void DecreaseFixtureCount()
    {
        SaveLoadSettings.SaveFixtureCount(Mathf.Max(minimumFixtures, FixtureCount - 1));
    }


    public void RebuildFixtures()
    {
        int targetCount = 1;
        if (DmxModeManager.Instance.CurrentMode == FixtureMode.Standard)
        {
            int defaultCount = Mathf.Clamp(minimumFixtures, 1, maximumFixtures);
            int savedCount = SaveLoadSettings.LoadInt(SaveLoadSettings.FixtureCountKey, defaultCount);
            targetCount = Mathf.Clamp(savedCount, minimumFixtures, maximumFixtures); 
        }
        RebuildFixtures(targetCount);
    }
    public void RebuildFixtures(int targetCount)
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
        UpdateFixtureCountDisplay(clampedCount);

    }

    public void RestoreSavedFixtureCount()
    {
        int defaultCount = Mathf.Clamp(minimumFixtures, 1, maximumFixtures);
        int savedCount = SaveLoadSettings.LoadInt(SaveLoadSettings.FixtureCountKey, defaultCount);
        RebuildFixtures(savedCount);
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
            instanceReceiver.Buffer = templateReceiver.Buffer;
        }

        _spawnedFixtures.Add(instance);

        for (int i = 0; i < _spawnedFixtures.Count; i++)
        {
            StartChannelOverride startChannelOverride = _spawnedFixtures[i].GetComponent<StartChannelOverride>();
            if (startChannelOverride == null)
            {
                startChannelOverride = _spawnedFixtures[i].AddComponent<StartChannelOverride>();
            }
            startChannelOverride.SetFixtureIndex(i);
            startChannelOverride.SetFixtureDmxChannelAmount(16);
        }
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
        SaveLoadSettings.SaveFixtureCount(count);

    }

    private void UpdateFixtureCountDisplay(int count)
    {
        if (fixtureCountValueText != null)
        {
            fixtureCountValueText.text = count.ToString();
        }
    }
}
