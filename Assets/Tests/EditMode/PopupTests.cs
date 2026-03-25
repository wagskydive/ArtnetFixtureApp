using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PopupTests
{
    [Test]
    public void OpenAndClose_ManagesSelectionAndVisibility()
    {
        var eventSystemGo = new GameObject("event-system");
        eventSystemGo.AddComponent<EventSystem>();
        eventSystemGo.AddComponent<StandaloneInputModule>();

        var backgroundButton = new GameObject("background-button").AddComponent<Button>();
        backgroundButton.gameObject.AddComponent<Image>();

        var popupRoot = new GameObject("popup-root");
        var popupButton = new GameObject("popup-button").AddComponent<Button>();
        popupButton.gameObject.AddComponent<Image>();
        popupButton.transform.SetParent(popupRoot.transform);

        var popup = popupRoot.AddComponent<Popup>();
        SetPrivateField(popup, "panelRoot", popupRoot);
        SetPrivateField(popup, "defaultSelection", popupButton);

        EventSystem.current.SetSelectedGameObject(backgroundButton.gameObject);
        popup.Open();

        Assert.That(popupRoot.activeSelf, Is.True);
        Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(popupButton.gameObject));

        popup.Close();

        Assert.That(popupRoot.activeSelf, Is.False);
        Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(backgroundButton.gameObject));

        Object.DestroyImmediate(backgroundButton.gameObject);
        Object.DestroyImmediate(popupRoot);
        Object.DestroyImmediate(eventSystemGo);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.SetValue(target, value);
    }
}
