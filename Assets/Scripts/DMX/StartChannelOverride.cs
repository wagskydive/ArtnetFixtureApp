using UnityEngine;


public class StartChannelOverride : MonoBehaviour
{

    private int fixtureIndex;
    private int fixtureDmxChannelAmount;

    public int FixtureIndex { get => fixtureIndex; }
    public int FixtureDmxChannelAmount { get => fixtureDmxChannelAmount; }

    public void SetFixtureIndex(int index)
    {
        fixtureIndex = index;
    }
    
    public void SetFixtureDmxChannelAmount(int amount)
    {
        fixtureDmxChannelAmount = amount;
    }

    public int GetChannelOffset()
    {
        return fixtureIndex * fixtureDmxChannelAmount;
    }

}