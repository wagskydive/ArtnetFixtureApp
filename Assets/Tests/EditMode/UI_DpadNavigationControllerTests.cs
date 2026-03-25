using NUnit.Framework;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_DpadNavigationControllerTests
{
    [Test]
    public void Move_WrapsSelectionAcrossConfiguredItems()
    {
        var eventSystemGo = CreateEventSystem();
        var root = CreateRootWithCanvas();
        var buttonA = CreateButton("a", root.transform, new Vector2(0f, 100f));
        var buttonB = CreateButton("b", root.transform, new Vector2(0f, 0f));
        var buttonC = CreateButton("c", root.transform, new Vector2(0f, -100f));

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
        var eventSystemGo = CreateEventSystem();
        var root = CreateRootWithCanvas();
        var disabledButton = CreateButton("disabled", root.transform, new Vector2(0f, 100f));
        disabledButton.interactable = false;
        var validButton = CreateButton("valid", root.transform, new Vector2(0f, 0f));

        var controller = root.AddComponent<UI_DpadNavigationController>();
        SetPrivateArray(controller, null, disabledButton, validButton);

        root.SendMessage("OnEnable");

        Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(validButton.gameObject));

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(eventSystemGo);
    }

    [Test]
    public void SubmitCurrentSelection_InvokesUI_DpadSelectableSubmitEvent_OncePerFrame()
    {
        var eventSystemGo = CreateEventSystem();
        var root = CreateRootWithCanvas();
        var submitGo = new GameObject("submit-item", typeof(RectTransform));
        submitGo.transform.SetParent(root.transform);
        var submitSelectable = submitGo.AddComponent<UI_DpadSelectable>();

        int invokeCount = 0;
        var eventField = typeof(UI_DpadSelectable)
            .GetField("onSubmit", BindingFlags.NonPublic | BindingFlags.Instance);
        var submitEvent = new UnityEngine.Events.UnityEvent();
        submitEvent.AddListener(() => invokeCount++);
        eventField.SetValue(submitSelectable, submitEvent);

        var controller = root.AddComponent<UI_DpadNavigationController>();
        SetPrivateArray(controller, submitSelectable);

        root.SendMessage("OnEnable");
        controller.SubmitCurrentSelection();
        controller.SubmitCurrentSelection();

        Assert.That(invokeCount, Is.EqualTo(1));

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(eventSystemGo);
    }

    [Test]
    public void HandleNavigationInput_DiscoversSelectablesByScreenPosition_WhenArrayNotConfigured()
    {
        var eventSystemGo = CreateEventSystem();
        var root = CreateRootWithCanvas();
        var top = CreateButton("top", root.transform, new Vector2(0f, 150f));
        var middle = CreateButton("middle", root.transform, new Vector2(0f, 0f));
        var bottom = CreateButton("bottom", root.transform, new Vector2(0f, -150f));

        var controller = root.AddComponent<UI_DpadNavigationController>();

        root.SendMessage("OnEnable");
        Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(top.gameObject));

        controller.HandleNavigationInput(Vector2.down);
        Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(middle.gameObject));

        controller.HandleNavigationInput(Vector2.down);
        Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(bottom.gameObject));

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(eventSystemGo);
    }

    [Test]
    public void HandleNavigationInput_IgnoresHorizontalWhenWrapDisabled()
    {
        var eventSystemGo = CreateEventSystem();
        var root = CreateRootWithCanvas();
        var buttonA = CreateButton("a", root.transform, new Vector2(0f, 0f));
        CreateButton("b", root.transform, new Vector2(100f, 0f));

        var controller = root.AddComponent<UI_DpadNavigationController>();

        root.SendMessage("OnEnable");
        controller.HandleNavigationInput(Vector2.right);

        Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(buttonA.gameObject));

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(eventSystemGo);
    }

    [Test]
    public void HandleNavigationInput_UsesHorizontalWhenEnabled()
    {
        var eventSystemGo = CreateEventSystem();
        var root = CreateRootWithCanvas();
        var buttonA = CreateButton("a", root.transform, new Vector2(0f, 0f));
        var buttonB = CreateButton("b", root.transform, new Vector2(100f, 0f));

        var controller = root.AddComponent<UI_DpadNavigationController>();
        SetPrivateField(controller, "allowHorizontalNavigation", true);

        root.SendMessage("OnEnable");
        EventSystem.current.SetSelectedGameObject(buttonA.gameObject);
        controller.HandleNavigationInput(Vector2.right);

        Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(buttonB.gameObject));

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(eventSystemGo);
    }

    [Test]
    public void HandleNavigationInput_WrapsVertically_WhenVerticalWrapEnabled()
    {
        var eventSystemGo = CreateEventSystem();
        var root = CreateRootWithCanvas();
        var top = CreateButton("top", root.transform, new Vector2(0f, 100f));
        var middle = CreateButton("middle", root.transform, new Vector2(0f, 0f));
        var bottom = CreateButton("bottom", root.transform, new Vector2(0f, -100f));

        var controller = root.AddComponent<UI_DpadNavigationController>();
        SetPrivateField(controller, "allowVerticalNavigation", true);
        SetPrivateField(controller, "verticalWrap", true);

        root.SendMessage("OnEnable");
        EventSystem.current.SetSelectedGameObject(bottom.gameObject);
        controller.HandleNavigationInput(Vector2.down);

        Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(top.gameObject));

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(eventSystemGo);
    }

    [Test]
    public void HandleNavigationInput_DoesNotMoveHorizontally_WhenHorizontalNavigationDisabled()
    {
        var eventSystemGo = CreateEventSystem();
        var root = CreateRootWithCanvas();
        var buttonA = CreateButton("a", root.transform, new Vector2(0f, 0f));
        CreateButton("b", root.transform, new Vector2(100f, 0f));

        var controller = root.AddComponent<UI_DpadNavigationController>();
        SetPrivateField(controller, "allowHorizontalNavigation", false);

        root.SendMessage("OnEnable");
        EventSystem.current.SetSelectedGameObject(buttonA.gameObject);
        controller.HandleNavigationInput(Vector2.right);

        Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(buttonA.gameObject));

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(eventSystemGo);
    }

    [Test]
    public void OnDisable_DoesNotDisableSharedNavigateAction_WhenAnotherControllerIsStillEnabled()
    {
        var eventSystemGo = CreateEventSystem();
        var rootA = CreateRootWithCanvas();
        var rootB = CreateRootWithCanvas();
        var a1 = CreateButton("a1", rootA.transform, new Vector2(0f, 0f));
        var a2 = CreateButton("a2", rootA.transform, new Vector2(100f, 0f));
        var b1 = CreateButton("b1", rootB.transform, new Vector2(0f, 0f));
        var b2 = CreateButton("b2", rootB.transform, new Vector2(100f, 0f));

        var navigateAction = new InputAction("navigate", InputActionType.Value);
        navigateAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        var navigateReference = InputActionReference.Create(navigateAction);

        var controllerA = rootA.AddComponent<UI_DpadNavigationController>();
        SetPrivateArray(controllerA, a1, a2);
        SetPrivateField(controllerA, "navigateAction", navigateReference);

        var controllerB = rootB.AddComponent<UI_DpadNavigationController>();
        SetPrivateArray(controllerB, b1, b2);
        SetPrivateField(controllerB, "navigateAction", navigateReference);

        rootA.SendMessage("OnEnable");
        rootB.SendMessage("OnEnable");
        Assert.That(navigateAction.enabled, Is.True);

        controllerA.enabled = false;
        Assert.That(navigateAction.enabled, Is.True);

        controllerB.enabled = false;
        Assert.That(navigateAction.enabled, Is.False);

        Object.DestroyImmediate(rootA);
        Object.DestroyImmediate(rootB);
        Object.DestroyImmediate(eventSystemGo);
    }

    private static GameObject CreateEventSystem()
    {
        var eventSystemGo = new GameObject("event-system");
        eventSystemGo.AddComponent<EventSystem>();
        eventSystemGo.AddComponent<StandaloneInputModule>();
        return eventSystemGo;
    }

    private static GameObject CreateRootWithCanvas()
    {
        var root = new GameObject("root", typeof(RectTransform), typeof(Canvas));
        root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        return root;
    }

    private static Button CreateButton(string name, Transform parent, Vector2 position)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent);
        var rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.localPosition = new Vector3(position.x, position.y, 0f);
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
