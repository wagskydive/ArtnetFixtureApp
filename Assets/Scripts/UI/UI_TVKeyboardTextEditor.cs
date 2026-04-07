using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class UI_TVKeyboardTextEditor : MonoBehaviour
{
    public enum ValidationMode
    {
        None = 0,
        IpAddress = 1,
        IntegerRange = 2,
        NoSpecialCharacters = 3,
        NoSpaces = 4,
        AlphanumericOnly = 5,
        LengthRange = 6
    }

    [SerializeField] private TVKeyboardTextFetcher tVKeyboardTextFetcher;
    [SerializeField] private Text textToEdit;
    [SerializeField] private ValidationMode validationMode = ValidationMode.None;
    [SerializeField] private Vector2Int integerRange = new Vector2Int(0, 255);
    [SerializeField] private Vector2Int lengthRange = new Vector2Int(1, 32);
    [SerializeField] private Popup validationPopup;
    [SerializeField] private Text validationErrorText;

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

    void OnDestroy()
    {
        if (tVKeyboardTextFetcher != null)
        {
            tVKeyboardTextFetcher.OnResult -= HandleResult;
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
        if (!TryValidate(result, out string validationMessage))
        {
            ShowValidationError(validationMessage);
            return;
        }

        HideValidationError();
        ApplyText(result);
    }

    void ApplyText(string text)
    {
        if (textToEdit != null)
        {
            textToEdit.text = text;
        }

        OnKeyboardApply.Invoke(text);
    }

    private bool TryValidate(string value, out string message)
    {
        message = string.Empty;
        string safeValue = value ?? string.Empty;

        switch (validationMode)
        {
            case ValidationMode.None:
                return true;
            case ValidationMode.IpAddress:
                return ValidateIpAddress(safeValue, out message);
            case ValidationMode.IntegerRange:
                return ValidateIntegerRange(safeValue, out message);
            case ValidationMode.NoSpecialCharacters:
                return ValidateNoSpecialCharacters(safeValue, out message);
            case ValidationMode.NoSpaces:
                return ValidateNoSpaces(safeValue, out message);
            case ValidationMode.AlphanumericOnly:
                return ValidateAlphanumericOnly(safeValue, out message);
            case ValidationMode.LengthRange:
                return ValidateLengthRange(safeValue, out message);
            default:
                return true;
        }
    }

    private bool ValidateIpAddress(string value, out string message)
    {
        bool valid = System.Net.IPAddress.TryParse(value, out System.Net.IPAddress parsedAddress) && parsedAddress != null;
        if (!valid)
        {
            message = "Enter a valid IP address (for example: 192.168.0.10).";
            return false;
        }

        message = string.Empty;
        return true;
    }

    private bool ValidateIntegerRange(string value, out string message)
    {
        Vector2Int normalizedRange = NormalizeRange(integerRange);
        if (!int.TryParse(value, out int parsedValue))
        {
            message = $"Enter a whole number between {normalizedRange.x} and {normalizedRange.y}.";
            return false;
        }

        if (parsedValue < normalizedRange.x || parsedValue > normalizedRange.y)
        {
            message = $"Number must be between {normalizedRange.x} and {normalizedRange.y}.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    private static bool ValidateNoSpecialCharacters(string value, out string message)
    {
        bool valid = Regex.IsMatch(value, @"^[a-zA-Z0-9 ]*$");
        if (!valid)
        {
            message = "Special characters are not allowed.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    private static bool ValidateNoSpaces(string value, out string message)
    {
        bool valid = !value.Contains(" ");
        if (!valid)
        {
            message = "Spaces are not allowed.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    private static bool ValidateAlphanumericOnly(string value, out string message)
    {
        bool valid = Regex.IsMatch(value, @"^[a-zA-Z0-9]*$");
        if (!valid)
        {
            message = "Use only letters and numbers.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    private bool ValidateLengthRange(string value, out string message)
    {
        Vector2Int normalizedRange = NormalizeRange(lengthRange);
        int length = value.Length;
        if (length < normalizedRange.x || length > normalizedRange.y)
        {
            message = $"Text length must be between {normalizedRange.x} and {normalizedRange.y} characters.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    private void ShowValidationError(string message)
    {
        if (validationErrorText != null)
        {
            validationErrorText.text = message;
        }

        if (validationPopup != null)
        {
            validationPopup.Open();
        }
    }

    private void HideValidationError()
    {
        if (validationPopup != null)
        {
            validationPopup.Close();
        }
    }

    private static Vector2Int NormalizeRange(Vector2Int range)
    {
        return range.x <= range.y ? range : new Vector2Int(range.y, range.x);
    }
}
