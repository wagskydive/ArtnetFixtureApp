using UnityEngine;

public class InfoTextFixtureMode : MonoBehaviour, IInfoText
{
    private string standardModeText;
    private string movingHeadModeText;
    private string pixelMappingModeText;

    public InfoTextsScriptableObject infoTexts; // assign in inspector

    void Start()
    {
        CacheTexts();
    }

    void CacheTexts()
    {
        if (infoTexts == null) return;

        // Use reflection to get the field dynamically
        var standardModeField = infoTexts.GetType().GetField("standardMode");
        if (standardModeField != null)
        {
            var value = standardModeField.GetValue(infoTexts);
            if (value != null)
            {
                standardModeText = value.ToString();
            }
        }
        var movingHeadModeField = infoTexts.GetType().GetField("movingHeadMode");
        if (movingHeadModeField != null)
        {
            var value = movingHeadModeField.GetValue(infoTexts);
            if (value != null)
            {
                movingHeadModeText = value.ToString();
            }
        }
        var pixelMappingModeField = infoTexts.GetType().GetField("pixelMappingMode");
        if (pixelMappingModeField != null)
        {
            var value = pixelMappingModeField.GetValue(infoTexts);
            if (value != null)
            {
                pixelMappingModeText = value.ToString();
            }
        }
    }

    public string GetInfoText()
    {

        switch (DmxModeManager.Instance.CurrentMode)
        {
            case DmxModeManager.FixtureMode.Standard:
                return standardModeText;
            case DmxModeManager.FixtureMode.MovingHead:
                return movingHeadModeText;
            case DmxModeManager.FixtureMode.PixelMapping:
                return pixelMappingModeText;
            default:
                return "DMX Mode unknown.";
        }
    }

}
