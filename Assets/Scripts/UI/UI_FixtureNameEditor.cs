using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;


public class UI_FixtureNameEditor : MonoBehaviour
{
    [SerializeField] private TVKeyboardTextFetcher tVKeyboardTextFetcher;
    [SerializeField] private Text nameText;



    void Awake()
    {
        if (tVKeyboardTextFetcher != null)
        {
            tVKeyboardTextFetcher.OnResult += HandleResult;
        }
    }

    public void ResetName()
    {
        if (tVKeyboardTextFetcher != null)
        {
            tVKeyboardTextFetcher.RequestAndroidTVKeyboard();
        }
    }

    void HandleResult(string result)
    {
        if (nameText != null)
        {
            nameText.text = result;
            ApplyFixtureName(result);
        }


    }
    public void ApplyFixtureName(string newName)
    {
        SaveLoadSettings.SaveString(SaveLoadSettings.DeviceNetworkKey, newName);
        SaveLoadSettings.Save();

    }
}

