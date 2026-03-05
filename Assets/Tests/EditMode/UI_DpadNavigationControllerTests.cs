using NUnit.Framework;
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

    private static Button CreateButton(string name)
    {
        var go = new GameObject(name);
        go.AddComponent<RectTransform>();
        return go.AddComponent<Button>();
    }

    private static void SetPrivateArray(UI_DpadNavigationController controller, params Selectable[] values)
    {
        typeof(UI_DpadNavigationController)
            .GetField("orderedSelectables", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(controller, values);
    }
}
