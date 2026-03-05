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

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(target, value);
    }
}
