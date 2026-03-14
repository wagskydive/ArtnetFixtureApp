using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;


public class UI_PasswordHelper : MonoBehaviour
{
    [SerializeField] private TVKeyboardTextFetcher tVKeyboardTextFetcher;
    [SerializeField] private UI_DmxSettings uI_DmxSettings;
    [SerializeField] private Text passwordText;



    void Awake()
    {
        if (tVKeyboardTextFetcher != null)
        {
            tVKeyboardTextFetcher.OnResult += HandleResult;
        }
    }

    public void ResetPassword()
    {
        if (tVKeyboardTextFetcher != null)
        {
            tVKeyboardTextFetcher.RequestAndroidTVKeyboard();
        }
    }

    void HandleResult(string result)
    {
        if (passwordText != null)
        {
            passwordText.text = result;
            ApplyPassword();
        }


    }
    public void ApplyPassword()
    {
        if (uI_DmxSettings != null)
        {
            uI_DmxSettings.ApplyWebUiPasswordFromInput();
        }
    }
}

