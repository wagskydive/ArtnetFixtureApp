using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class UI_TVKeyboardTextEditorTests
{
    [Test]
    public void HandleResult_IntegerRangeValid_UpdatesText()
    {
        var host = new GameObject("editor-host");
        var targetText = new GameObject("target-text").AddComponent<Text>();
        var editor = host.AddComponent<UI_TVKeyboardTextEditor>();

        SetPrivateField(editor, "textToEdit", targetText);
        SetPrivateField(editor, "validationMode", UI_TVKeyboardTextEditor.ValidationMode.IntegerRange);
        SetPrivateField(editor, "integerRange", new Vector2Int(10, 20));

        InvokePrivateMethod(editor, "HandleResult", "12");

        Assert.That(targetText.text, Is.EqualTo("12"));

        Object.DestroyImmediate(targetText.gameObject);
        Object.DestroyImmediate(host);
    }

    [Test]
    public void HandleResult_IntegerRangeInvalid_ShowsValidationPopupAndKeepsPreviousText()
    {
        var host = new GameObject("editor-host");
        var targetText = new GameObject("target-text").AddComponent<Text>();
        targetText.text = "existing";

        var popupRoot = new GameObject("validation-popup");
        popupRoot.SetActive(false);
        var popup = popupRoot.AddComponent<Popup>();
        SetPrivateField(popup, "panelRoot", popupRoot);

        var messageText = new GameObject("message-text").AddComponent<Text>();

        var editor = host.AddComponent<UI_TVKeyboardTextEditor>();
        SetPrivateField(editor, "textToEdit", targetText);
        SetPrivateField(editor, "validationMode", UI_TVKeyboardTextEditor.ValidationMode.IntegerRange);
        SetPrivateField(editor, "integerRange", new Vector2Int(10, 20));
        SetPrivateField(editor, "validationPopup", popup);
        SetPrivateField(editor, "validationErrorText", messageText);

        InvokePrivateMethod(editor, "HandleResult", "999");

        Assert.That(targetText.text, Is.EqualTo("existing"));
        Assert.That(popupRoot.activeSelf, Is.True);
        Assert.That(messageText.text, Does.Contain("between 10 and 20"));

        Object.DestroyImmediate(messageText.gameObject);
        Object.DestroyImmediate(popupRoot);
        Object.DestroyImmediate(targetText.gameObject);
        Object.DestroyImmediate(host);
    }

    [Test]
    public void HandleResult_NoSpacesInvalid_DoesNotApplyText()
    {
        var host = new GameObject("editor-host");
        var targetText = new GameObject("target-text").AddComponent<Text>();
        targetText.text = "before";
        var editor = host.AddComponent<UI_TVKeyboardTextEditor>();

        SetPrivateField(editor, "textToEdit", targetText);
        SetPrivateField(editor, "validationMode", UI_TVKeyboardTextEditor.ValidationMode.NoSpaces);

        InvokePrivateMethod(editor, "HandleResult", "with space");

        Assert.That(targetText.text, Is.EqualTo("before"));

        Object.DestroyImmediate(targetText.gameObject);
        Object.DestroyImmediate(host);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.SetValue(target, value);
    }

    private static void InvokePrivateMethod(object target, string methodName, string argument)
    {
        var method = target.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(target, new object[] { argument });
    }
}
