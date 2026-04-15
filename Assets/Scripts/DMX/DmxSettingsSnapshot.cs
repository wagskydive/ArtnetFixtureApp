using UnityEngine;

public readonly struct DmxSettingsSnapshot
{
    public readonly int Universe1Based;
    public readonly int Universe0Based;
    public readonly int StartChannel;

    public readonly bool IsSAcnMode;


    public  SAcnParameters CurrentSAcnParameters {get => GetParameters();}

    private readonly SAcnParameters currentSAcnParameters;

    public SAcnParameters GetParameters()
    {
        if(currentSAcnParameters == null)
        {
            return SAcnParameters.Default();
        }
        return currentSAcnParameters;
    }

    public DmxSettingsSnapshot(
        int universe1Based,
        int startChannel,
        bool isSAcnMode,
        SAcnParameters parameters)
    {

        universe1Based = DmxSettingsService.ClampUniverse(universe1Based, isSAcnMode);

        Universe1Based = universe1Based;
        Universe0Based = Mathf.Max(0, universe1Based - 1);
        StartChannel = Mathf.Clamp(startChannel, 1, 512);

        IsSAcnMode = isSAcnMode;
        currentSAcnParameters = parameters;

    }

    public DmxSettingsSnapshot(int newUniverse1BasedOnly, DmxSettingsSnapshot oldSnapshot)
    {
        newUniverse1BasedOnly = DmxSettingsService.ClampUniverse(newUniverse1BasedOnly, oldSnapshot.IsSAcnMode);
        Universe1Based = newUniverse1BasedOnly;
        Universe0Based = Mathf.Max(0, newUniverse1BasedOnly - 1);

        StartChannel = oldSnapshot.StartChannel;
        IsSAcnMode = oldSnapshot.IsSAcnMode;

        currentSAcnParameters = SAcnParameters.Clone(oldSnapshot.CurrentSAcnParameters);
    }

    public DmxSettingsSnapshot(DmxSettingsSnapshot oldSnapshot, int newStartChannelOnly)
    {
        Universe1Based = oldSnapshot.Universe1Based;
        Universe0Based = Mathf.Max(0, oldSnapshot.Universe1Based - 1);

        StartChannel = Mathf.Clamp(newStartChannelOnly, 1, 512);
        IsSAcnMode = oldSnapshot.IsSAcnMode;

        currentSAcnParameters = SAcnParameters.Clone(oldSnapshot.CurrentSAcnParameters);
    }

    public DmxSettingsSnapshot(bool newIsSAcnModeOnly, DmxSettingsSnapshot oldSnapshot)
    {
        Universe1Based = oldSnapshot.Universe1Based;
        Universe0Based = Mathf.Max(0, oldSnapshot.Universe1Based - 1);

        StartChannel = oldSnapshot.StartChannel;
        IsSAcnMode = newIsSAcnModeOnly;

        currentSAcnParameters = SAcnParameters.Clone(oldSnapshot.CurrentSAcnParameters);
    }

    public DmxSettingsSnapshot(SAcnParameters newSAcnParametersOnly, DmxSettingsSnapshot oldSnapshot)
    {
        Universe1Based = oldSnapshot.Universe1Based;
        Universe0Based = Mathf.Max(0, oldSnapshot.Universe1Based - 1);

        StartChannel = oldSnapshot.StartChannel;
        IsSAcnMode = oldSnapshot.IsSAcnMode;

        currentSAcnParameters = SAcnParameters.Clone(newSAcnParametersOnly);
    }

}