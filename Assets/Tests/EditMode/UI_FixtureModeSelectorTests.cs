using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class UI_FixtureModeSelectorTests
{
    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey("dmx.fixture.mode");
        PlayerPrefs.DeleteKey("dmx.pixel.rows");
        PlayerPrefs.DeleteKey("dmx.pixel.columns");
    }

    [Test]
    public void SetMode_SwitchesMaterialToMovingHead()
    {
        var selector = CreateSelector(out var renderer, out var standardMaterial, out var movingMaterial, out var pixelMaterial, out _, out _, out _);

        selector.SetMode(UI_FixtureModeSelector.FixtureMode.MovingHead);

        Assert.That(selector.CurrentMode, Is.EqualTo(UI_FixtureModeSelector.FixtureMode.MovingHead));
        Assert.That(renderer.sharedMaterial, Is.EqualTo(movingMaterial));

        DestroySelector(selector, standardMaterial, movingMaterial, pixelMaterial);
    }

    [Test]
    public void SetMode_SwitchesMaterialToPixelMapping()
    {
        var selector = CreateSelector(out var renderer, out var standardMaterial, out var movingMaterial, out var pixelMaterial, out _, out _, out _);

        selector.SetMode(UI_FixtureModeSelector.FixtureMode.PixelMapping);

        Assert.That(selector.CurrentMode, Is.EqualTo(UI_FixtureModeSelector.FixtureMode.PixelMapping));
        Assert.That(renderer.sharedMaterial, Is.EqualTo(pixelMaterial));

        DestroySelector(selector, standardMaterial, movingMaterial, pixelMaterial);
    }

    [Test]
    public void SaveAndLoadPreferences_RestoresSelectedModeAndGridSize()
    {
        var selector = CreateSelector(out _, out var standardMaterial, out var movingMaterial, out var pixelMaterial, out _, out _, out _);
        selector.SetMode(UI_FixtureModeSelector.FixtureMode.PixelMapping);
        selector.CurrentPixelRows = 14;
        selector.CurrentPixelColumns = 18;
        selector.SavePreferences();

        var reloaded = CreateSelector(out _, out var standardReloadMaterial, out var movingReloadMaterial, out var pixelReloadMaterial, out _, out var rowsText, out var columnsText);
        reloaded.LoadPreferences();
        reloaded.SendMessage("Start");

        Assert.That(reloaded.CurrentMode, Is.EqualTo(UI_FixtureModeSelector.FixtureMode.PixelMapping));
        Assert.That(reloaded.CurrentPixelRows, Is.EqualTo(14));
        Assert.That(reloaded.CurrentPixelColumns, Is.EqualTo(18));
        Assert.That(rowsText.text, Is.EqualTo("14"));
        Assert.That(columnsText.text, Is.EqualTo("18"));

        DestroySelector(selector, standardMaterial, movingMaterial, pixelMaterial);
        DestroySelector(reloaded, standardReloadMaterial, movingReloadMaterial, pixelReloadMaterial);
    }

    [Test]
    public void PixelGridSize_ClampsBetweenOneAndThirtyTwo()
    {
        var selector = CreateSelector(out var renderer, out var standardMaterial, out var movingMaterial, out var pixelMaterial, out _, out var rowsText, out var columnsText);
        selector.SendMessage("Start");
        selector.SetMode(UI_FixtureModeSelector.FixtureMode.PixelMapping);

        selector.CurrentPixelRows = 0;
        selector.CurrentPixelColumns = 99;

        Assert.That(selector.CurrentPixelRows, Is.EqualTo(1));
        Assert.That(selector.CurrentPixelColumns, Is.EqualTo(32));
        Assert.That(rowsText.text, Is.EqualTo("1"));
        Assert.That(columnsText.text, Is.EqualTo("32"));
        Assert.That(pixelMaterial.GetFloat("_Rows"), Is.EqualTo(1f));
        Assert.That(pixelMaterial.GetFloat("_Columns"), Is.EqualTo(32f));
        Assert.That(renderer.sharedMaterial.GetFloat("_Rows"), Is.EqualTo(1f));
        Assert.That(renderer.sharedMaterial.GetFloat("_Columns"), Is.EqualTo(32f));

        DestroySelector(selector, standardMaterial, movingMaterial, pixelMaterial);
    }

    [Test]
    public void Start_SyncsDropdownFromSavedMode()
    {
        PlayerPrefs.SetInt("dmx.fixture.mode", 2);

        var selector = CreateSelector(out _, out var standardMaterial, out var movingMaterial, out var pixelMaterial, out var dropdown, out _, out _);

        selector.SendMessage("Start");

        Assert.That(dropdown.value, Is.EqualTo(2));
        Assert.That(selector.CurrentMode, Is.EqualTo(UI_FixtureModeSelector.FixtureMode.PixelMapping));

        DestroySelector(selector, standardMaterial, movingMaterial, pixelMaterial);
    }

    private static UI_FixtureModeSelector CreateSelector(
        out Renderer renderer,
        out Material standardMaterial,
        out Material movingMaterial,
        out Material pixelMaterial,
        out Dropdown dropdown,
        out Text pixelRowsValue,
        out Text pixelColumnsValue)
    {
        var root = new GameObject("mode-root");
        renderer = root.AddComponent<MeshRenderer>();
        root.AddComponent<MeshFilter>();

        var selector = root.AddComponent<UI_FixtureModeSelector>();

        standardMaterial = new Material(Shader.Find("Standard"));
        movingMaterial = new Material(Shader.Find("Standard"));
        pixelMaterial = new Material(Shader.Find("Standard"));

        var dropdownGo = new GameObject("dropdown");
        dropdownGo.AddComponent<RectTransform>();
        dropdown = dropdownGo.AddComponent<Dropdown>();
        dropdown.options.Add(new Dropdown.OptionData("Standard"));
        dropdown.options.Add(new Dropdown.OptionData("Moving Head"));
        dropdown.options.Add(new Dropdown.OptionData("Pixel Mapping"));

        var rowsTextGo = new GameObject("rows-text");
        pixelRowsValue = rowsTextGo.AddComponent<Text>();

        var columnsTextGo = new GameObject("columns-text");
        pixelColumnsValue = columnsTextGo.AddComponent<Text>();

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
            .GetField("pixelMappingModeMaterial", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(selector, pixelMaterial);

        typeof(UI_FixtureModeSelector)
            .GetField("modeDropdown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(selector, dropdown);

        typeof(UI_FixtureModeSelector)
            .GetField("pixelRowsValueText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(selector, pixelRowsValue);

        typeof(UI_FixtureModeSelector)
            .GetField("pixelColumnsValueText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(selector, pixelColumnsValue);

        return selector;
    }

    private static void DestroySelector(UI_FixtureModeSelector selector, Material standardMaterial, Material movingMaterial, Material pixelMaterial)
    {
        Object.DestroyImmediate(standardMaterial);
        Object.DestroyImmediate(movingMaterial);
        Object.DestroyImmediate(pixelMaterial);
        Object.DestroyImmediate(selector.gameObject);
    }
}
