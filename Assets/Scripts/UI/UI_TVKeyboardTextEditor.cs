using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class UI_TVKeyboardTextEditor : MonoBehaviour
{
    [SerializeField] private TVKeyboardTextFetcher tVKeyboardTextFetcher;
    [SerializeField] private Text textToEdit;

    [System.Serializable]
    public class TVKeyboardApplyEvent : UnityEvent<string> { }

    [SerializeField]
    private TVKeyboardApplyEvent OnKeyboardApply = new TVKeyboardApplyEvent();

    void Awake()
    {
        if (tVKeyboardTextFetcher != null)
        {
            tVKeyboardTextFetcher.OnResult += HandleResult;
        }
    }

    public void EditText()
    {
        if (tVKeyboardTextFetcher != null)
        {
            tVKeyboardTextFetcher.RequestAndroidTVKeyboard();
        }
    }

    void HandleResult(string result)
    {
        if (textToEdit != null)
        {
            textToEdit.text = result;
            ApplyText(result);
        }


    }
    void ApplyText(string text)
    {
        if(textToEdit != null)
        {
            textToEdit.text = text;
        }
        
        OnKeyboardApply.Invoke(text);
    }
}

