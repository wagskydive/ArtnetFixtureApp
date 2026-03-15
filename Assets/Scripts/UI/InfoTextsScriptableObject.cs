using UnityEngine;

[CreateAssetMenu(fileName = "InfoTexts", menuName = "InfoTexts", order = 1)]
public class InfoTextsScriptableObject : ScriptableObject
{
    [TextArea] public string generalInfo;
    [TextArea] public string fixtureName;
    [TextArea] public string ipAddress;
    [TextArea] public string webUi;
    [TextArea] public string fixtureModeGeneral;
    [TextArea] public string standardMode;
    [TextArea] public string movingHeadMode;
    [TextArea] public string pixelMappingMode;
    [TextArea] public string universe;
    [TextArea] public string startChannel;
    [TextArea] public string performance;
    [TextArea] public string fixtureAmount;
    [TextArea] public string pixelRows;
    [TextArea] public string pixelColumns;
    [TextArea] public string networkWarning;
    [TextArea] public string passwordProtection;
    [TextArea] public string infoPanel;

    [TextArea] public string infoPanelDefault;

    // Add more entries as needed
}