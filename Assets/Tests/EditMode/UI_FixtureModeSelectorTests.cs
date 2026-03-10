using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class UI_FixtureModeSelectorTests
{
    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey("dmx.fixture.mode");
    }

    [Test]
    public void SetMode_SwitchesMaterialToMovingHead()
    {
        var selector = CreateSelector(out var renderer, out var standardMaterial, out var movingMaterial, out _);

        selector.SetMode(UI_FixtureModeSelector.FixtureMode.MovingHead);

        Assert.That(selector.CurrentMode, Is.EqualTo(UI_FixtureModeSelector.FixtureMode.MovingHead));
        Assert.That(renderer.sharedMaterial, Is.EqualTo(movingMaterial));

        Object.DestroyImmediate(standardMaterial);
        Object.DestroyImmediate(movingMaterial);
        Object.DestroyImmediate(selector.gameObject);
    }

    [Test]
    public void SaveAndLoadPreferences_RestoresSelectedMode()
    {
        var selector = CreateSelector(out _, out var standardMaterial, out var movingMaterial, out _);
        selector.SetMode(UI_FixtureModeSelector.FixtureMode.MovingHead);
        selector.SavePreferences();

        var reloaded = CreateSelector(out _, out var standardReloadMaterial, out var movingReloadMaterial, out _);
        reloaded.LoadPreferences();
        reloaded.SendMessage("Start");

        Assert.That(reloaded.CurrentMode, Is.EqualTo(UI_FixtureModeSelector.FixtureMode.MovingHead));

        Object.DestroyImmediate(standardMaterial);
        Object.DestroyImmediate(movingMaterial);
        Object.DestroyImmediate(selector.gameObject);
        Object.DestroyImmediate(standardReloadMaterial);
        Object.DestroyImmediate(movingReloadMaterial);
        Object.DestroyImmediate(reloaded.gameObject);
    }

    [Test]
    public void Start_SyncsDropdownFromSavedMode()
    {
        PlayerPrefs.SetInt("dmx.fixture.mode", 1);

        var selector = CreateSelector(out _, out var standardMaterial, out var movingMaterial, out var dropdown);

        selector.SendMessage("Start");

        Assert.That(dropdown.value, Is.EqualTo(1));
        Assert.That(selector.CurrentMode, Is.EqualTo(UI_FixtureModeSelector.FixtureMode.MovingHead));

        Object.DestroyImmediate(standardMaterial);
        Object.DestroyImmediate(movingMaterial);
        Object.DestroyImmediate(selector.gameObject);
    }

    private static UI_FixtureModeSelector CreateSelector(out Renderer renderer, out Material standardMaterial, out Material movingMaterial, out Dropdown dropdown)
    {
        var root = new GameObject("mode-root");
        renderer = root.AddComponent<MeshRenderer>();
        root.AddComponent<MeshFilter>();

        var selector = root.AddComponent<UI_FixtureModeSelector>();

        standardMaterial = new Material(Shader.Find("Standard"));
        movingMaterial = new Material(Shader.Find("Standard"));

        var dropdownGo = new GameObject("dropdown");
        dropdownGo.AddComponent<RectTransform>();
        dropdown = dropdownGo.AddComponent<Dropdown>();
        dropdown.options.Add(new Dropdown.OptionData("Standard"));
        dropdown.options.Add(new Dropdown.OptionData("Moving Head"));

        typeof(UI_FixtureModeSelector)
            .GetField("targetRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(selector, renderer);

        typeof(UI_FixtureModeSelector)
            .GetField("standardModeMaterial", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(selector, standardMaterial);

        typeof(UI_FixtureModeSelector)
            .GetField("movingHeadModeMaterial", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(selector, movingMaterial);

        typeof(UI_FixtureModeSelector)
            .GetField("modeDropdown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(selector, dropdown);

        return selector;
    }
}
