using NUnit.Framework;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_DpadNavigationControllerTests
{
    [Test]
    public void Move_WrapsSelectionAcrossConfiguredItems()
    {
        var eventSystemGo = new GameObject("event-system");
        eventSystemGo.AddComponent<EventSystem>();
        eventSystemGo.AddComponent<StandaloneInputModule>();

        var root = new GameObject("root");
        var buttonA = CreateButton("a");
        var buttonB = CreateButton("b");
        var buttonC = CreateButton("c");

        buttonA.transform.SetParent(root.transform);
        buttonB.transform.SetParent(root.transform);
        buttonC.transform.SetParent(root.transform);

        var controller = root.AddComponent<UI_DpadNavigationController>();
        SetPrivateArray(controller, buttonA, buttonB, buttonC);

        root.SendMessage("OnEnable");
        controller.Move(-1);

        Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(buttonC.gameObject));

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(eventSystemGo);
    }

    [Test]
    public void OnEnable_SelectsFirstInteractableItem_WhenArrayStartsWithNullOrDisabled()
    {
        var eventSystemGo = new GameObject("event-system");
        eventSystemGo.AddComponent<EventSystem>();
        eventSystemGo.AddComponent<StandaloneInputModule>();

        var root = new GameObject("root");
        var disabledButton = CreateButton("disabled");
        disabledButton.interactable = false;
        var validButton = CreateButton("valid");

        disabledButton.transform.SetParent(root.transform);
        validButton.transform.SetParent(root.transform);

        var controller = root.AddComponent<UI_DpadNavigationController>();
        SetPrivateArray(controller, null, disabledButton, validButton);

        root.SendMessage("OnEnable");

        Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(validButton.gameObject));

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(eventSystemGo);
    }

    [Test]
    public void SubmitCurrentSelection_InvokesUI_DpadSelectableSubmitEvent()
    {
        var eventSystemGo = new GameObject("event-system");
        eventSystemGo.AddComponent<EventSystem>();
        eventSystemGo.AddComponent<StandaloneInputModule>();

        var root = new GameObject("root");
        var submitGo = new GameObject("submit-item");
        submitGo.AddComponent<RectTransform>();
        var submitSelectable = submitGo.AddComponent<UI_DpadSelectable>();
        submitGo.transform.SetParent(root.transform);

        bool invoked = false;
        var eventField = typeof(UI_DpadSelectable)
            .GetField("onSubmit", BindingFlags.NonPublic | BindingFlags.Instance);
        var submitEvent = new UnityEngine.Events.UnityEvent();
        submitEvent.AddListener(() => invoked = true);
        eventField.SetValue(submitSelectable, submitEvent);

        var controller = root.AddComponent<UI_DpadNavigationController>();
        SetPrivateArray(controller, submitSelectable);

        root.SendMessage("OnEnable");
        controller.SubmitCurrentSelection();

        Assert.That(invoked, Is.True);

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(eventSystemGo);
    }

    [Test]
    public void HandleNavigationInput_UsesVerticalAxisByDefault()
    {
        var eventSystemGo = new GameObject("event-system");
        eventSystemGo.AddComponent<EventSystem>();
        eventSystemGo.AddComponent<StandaloneInputModule>();

        var root = new GameObject("root");
        var buttonA = CreateButton("a");
        var buttonB = CreateButton("b");

        buttonA.transform.SetParent(root.transform);
        buttonB.transform.SetParent(root.transform);

        var controller = root.AddComponent<UI_DpadNavigationController>();
        SetPrivateArray(controller, buttonA, buttonB);

        root.SendMessage("OnEnable");
        controller.HandleNavigationInput(Vector2.down);

        Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(buttonB.gameObject));

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(eventSystemGo);
    }

    [Test]
    public void HandleNavigationInput_IgnoresHorizontalWhenWrapDisabled()
    {
        var eventSystemGo = new GameObject("event-system");
        eventSystemGo.AddComponent<EventSystem>();
        eventSystemGo.AddComponent<StandaloneInputModule>();

        var root = new GameObject("root");
        var buttonA = CreateButton("a");
        var buttonB = CreateButton("b");

        buttonA.transform.SetParent(root.transform);
        buttonB.transform.SetParent(root.transform);

        var controller = root.AddComponent<UI_DpadNavigationController>();
        SetPrivateArray(controller, buttonA, buttonB);

        root.SendMessage("OnEnable");
        controller.HandleNavigationInput(Vector2.right);

        Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(buttonA.gameObject));

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(eventSystemGo);
    }

    [Test]
    public void HandleNavigationInput_UsesHorizontalWhenWrapEnabled()
    {
        var eventSystemGo = new GameObject("event-system");
        eventSystemGo.AddComponent<EventSystem>();
        eventSystemGo.AddComponent<StandaloneInputModule>();

        var root = new GameObject("root");
        var buttonA = CreateButton("a");
        var buttonB = CreateButton("b");

        buttonA.transform.SetParent(root.transform);
        buttonB.transform.SetParent(root.transform);

        var controller = root.AddComponent<UI_DpadNavigationController>();
        SetPrivateArray(controller, buttonA, buttonB);
        SetPrivateField(controller, "horizontalWrap", true);

        root.SendMessage("OnEnable");
        controller.HandleNavigationInput(Vector2.right);

        Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(buttonB.gameObject));

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(eventSystemGo);
    }

    private static Button CreateButton(string name)
    {
        var go = new GameObject(name);
        go.AddComponent<RectTransform>();
        return go.AddComponent<Button>();
    }

    private static void SetPrivateArray(UI_DpadNavigationController controller, params Selectable[] values)
    {
        typeof(UI_DpadNavigationController)
            .GetField("orderedSelectables", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(controller, values);
    }

    private static void SetPrivateField(UI_DpadNavigationController controller, string fieldName, object value)
    {
        typeof(UI_DpadNavigationController)
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(controller, value);
    }
}
