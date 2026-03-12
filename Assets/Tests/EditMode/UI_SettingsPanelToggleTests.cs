using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class UI_SettingsPanelToggleTests
{
    [Test]
    public void ToggleTargetVisibility_HidesActiveObject()
    {
        var controllerGo = new GameObject("controller");
        var targetGo = new GameObject("target");
        targetGo.SetActive(true);

        var toggle = controllerGo.AddComponent<UI_SettingsPanelToggle>();
        SetPrivateField(toggle, "targetObject", targetGo);

        toggle.ToggleTargetVisibility();

        Assert.That(targetGo.activeSelf, Is.False);

        Object.DestroyImmediate(controllerGo);
        Object.DestroyImmediate(targetGo);
    }

    [Test]
    public void ToggleTargetVisibility_ShowsInactiveObject()
    {
        var controllerGo = new GameObject("controller");
        var targetGo = new GameObject("target");
        targetGo.SetActive(false);

        var toggle = controllerGo.AddComponent<UI_SettingsPanelToggle>();
        SetPrivateField(toggle, "targetObject", targetGo);

        toggle.ToggleTargetVisibility();

        Assert.That(targetGo.activeSelf, Is.True);

        Object.DestroyImmediate(controllerGo);
        Object.DestroyImmediate(targetGo);
    }

    [Test]
    public void HideMenu_KeepTargetObjectActiveWhenHidden_KeepsObjectActive()
    {
        var controllerGo = new GameObject("controller");
        var targetGo = new GameObject("target");
        targetGo.SetActive(true);

        var toggle = controllerGo.AddComponent<UI_SettingsPanelToggle>();
        SetPrivateField(toggle, "targetObject", targetGo);
        SetPrivateField(toggle, "keepTargetObjectActiveWhenHidden", true);

        toggle.HideMenu();

        Assert.That(targetGo.activeSelf, Is.True);

        Object.DestroyImmediate(controllerGo);
        Object.DestroyImmediate(targetGo);
    }

    [Test]
    public void HideMenu_DefaultBehavior_HidesObject()
    {
        var controllerGo = new GameObject("controller");
        var targetGo = new GameObject("target");
        targetGo.SetActive(true);

        var toggle = controllerGo.AddComponent<UI_SettingsPanelToggle>();
        SetPrivateField(toggle, "targetObject", targetGo);

        toggle.HideMenu();

        Assert.That(targetGo.activeSelf, Is.False);

        Object.DestroyImmediate(controllerGo);
        Object.DestroyImmediate(targetGo);
    }

    [Test]
    public void Start_ShowMenuOnStartFalse_HidesObject()
    {
        var controllerGo = new GameObject("controller");
        var targetGo = new GameObject("target");
        targetGo.SetActive(true);

        var toggle = controllerGo.AddComponent<UI_SettingsPanelToggle>();
        SetPrivateField(toggle, "targetObject", targetGo);
        SetPrivateField(toggle, "showMenuOnStart", false);

        InvokePrivateMethod(toggle, "Start");

        Assert.That(targetGo.activeSelf, Is.False);

        Object.DestroyImmediate(controllerGo);
        Object.DestroyImmediate(targetGo);
    }

    [Test]
    public void Start_ShowMenuOnStartTrue_ShowsObject()
    {
        var controllerGo = new GameObject("controller");
        var targetGo = new GameObject("target");
        targetGo.SetActive(false);

        var toggle = controllerGo.AddComponent<UI_SettingsPanelToggle>();
        SetPrivateField(toggle, "targetObject", targetGo);
        SetPrivateField(toggle, "showMenuOnStart", true);

        InvokePrivateMethod(toggle, "Start");

        Assert.That(targetGo.activeSelf, Is.True);

        Object.DestroyImmediate(controllerGo);
        Object.DestroyImmediate(targetGo);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(target, value);
    }

    private static void InvokePrivateMethod(object target, string methodName)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(target, null);
    }
}
