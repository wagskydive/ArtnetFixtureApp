using UnityEngine;

public class InfoTextDefault : MonoBehaviour, IInfoText
{
    public InfoTextsScriptableObject infoTexts; // assign in inspector
    public string key; // the name of the field to use

    public string GetInfoText()
    {
        if (infoTexts == null || string.IsNullOrEmpty(key)) return "";

        // Use reflection to get the field dynamically
        var field = infoTexts.GetType().GetField(key);
        if (field != null)
        {
            var value = field.GetValue(infoTexts);
            return value != null ? value.ToString() : "";
        }

        return "";
    }

}
