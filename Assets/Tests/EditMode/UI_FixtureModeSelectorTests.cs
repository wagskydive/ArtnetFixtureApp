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
        var selector = CreateSelector(out var renderer, out var standardMaterial, out var movingMaterial, out var pixelMaterial, out _, out _, out _, out _);

        selector.SetMode(UI_FixtureModeSelector.FixtureMode.MovingHead);

        Assert.That(selector.CurrentMode, Is.EqualTo(UI_FixtureModeSelector.FixtureMode.MovingHead));
        Assert.That(renderer.sharedMaterial, Is.EqualTo(movingMaterial));

        DestroySelector(selector, standardMaterial, movingMaterial, pixelMaterial);
    }

    [Test]
    public void SetMode_SwitchesMaterialToPixelMapping()
    {
        var selector = CreateSelector(out var renderer, out var standardMaterial, out var movingMaterial, out var pixelMaterial, out _, out _, out _, out _);

        selector.SetMode(UI_FixtureModeSelector.FixtureMode.PixelMapping);

        Assert.That(selector.CurrentMode, Is.EqualTo(UI_FixtureModeSelector.FixtureMode.PixelMapping));
        Assert.That(renderer.sharedMaterial, Is.EqualTo(pixelMaterial));

        DestroySelector(selector, standardMaterial, movingMaterial, pixelMaterial);
    }

    [Test]
    public void IncreaseAndDecreaseMode_CyclesAcrossModes()
    {
        var selector = CreateSelector(out _, out var standardMaterial, out var movingMaterial, out var pixelMaterial, out var modeText, out _, out _, out _);
        selector.SendMessage("Start");

        selector.DecreaseMode();
        Assert.That(selector.CurrentMode, Is.EqualTo(UI_FixtureModeSelector.FixtureMode.PixelMapping));
        Assert.That(modeText.text, Is.EqualTo("Pixel Mapping"));

        selector.IncreaseMode();
        Assert.That(selector.CurrentMode, Is.EqualTo(UI_FixtureModeSelector.FixtureMode.Standard));
        Assert.That(modeText.text, Is.EqualTo("Standard"));

        DestroySelector(selector, standardMaterial, movingMaterial, pixelMaterial);
    }

    [Test]
    public void SaveAndLoadPreferences_RestoresSelectedModeAndGridSize()
    {
        var selector = CreateSelector(out _, out var standardMaterial, out var movingMaterial, out var pixelMaterial, out _, out _, out _, out _);
        selector.SetMode(UI_FixtureModeSelector.FixtureMode.PixelMapping);
        selector.CurrentPixelRows = 24;
        selector.CurrentPixelColumns = 32;
        selector.SavePreferences();

        var reloaded = CreateSelector(out _, out var standardReloadMaterial, out var movingReloadMaterial, out var pixelReloadMaterial, out var modeText, out var pixelGridContainer, out var rowsText, out var columnsText);
        reloaded.LoadPreferences();
        reloaded.SendMessage("Start");

        Assert.That(reloaded.CurrentMode, Is.EqualTo(UI_FixtureModeSelector.FixtureMode.PixelMapping));
        Assert.That(reloaded.CurrentPixelRows, Is.EqualTo(24));
        Assert.That(reloaded.CurrentPixelColumns, Is.EqualTo(32));
        Assert.That(modeText.text, Is.EqualTo("Pixel Mapping"));
        Assert.That(pixelGridContainer.activeSelf, Is.True);
        Assert.That(rowsText.text, Is.EqualTo("24"));
        Assert.That(columnsText.text, Is.EqualTo("32"));

        DestroySelector(selector, standardMaterial, movingMaterial, pixelMaterial);
        DestroySelector(reloaded, standardReloadMaterial, movingReloadMaterial, pixelReloadMaterial);
    }

    [Test]
    public void PixelGridSize_UsesEightStepAndClampsBetweenEightAndThirtyTwo()
    {
        var selector = CreateSelector(out var renderer, out var standardMaterial, out var movingMaterial, out var pixelMaterial, out _, out _, out var rowsText, out var columnsText);
        selector.SendMessage("Start");
        selector.SetMode(UI_FixtureModeSelector.FixtureMode.PixelMapping);

        selector.CurrentPixelRows = 4;
        selector.CurrentPixelColumns = 99;

        Assert.That(selector.CurrentPixelRows, Is.EqualTo(8));
        Assert.That(selector.CurrentPixelColumns, Is.EqualTo(32));

        selector.IncreasePixelRows();
        selector.DecreasePixelColumns();

        Assert.That(selector.CurrentPixelRows, Is.EqualTo(16));
        Assert.That(selector.CurrentPixelColumns, Is.EqualTo(24));
        Assert.That(rowsText.text, Is.EqualTo("16"));
        Assert.That(columnsText.text, Is.EqualTo("24"));
        Assert.That(pixelMaterial.GetFloat("_Rows"), Is.EqualTo(16f));
        Assert.That(pixelMaterial.GetFloat("_Columns"), Is.EqualTo(24f));
        Assert.That(renderer.sharedMaterial.GetFloat("_Rows"), Is.EqualTo(16f));
        Assert.That(renderer.sharedMaterial.GetFloat("_Columns"), Is.EqualTo(24f));

        DestroySelector(selector, standardMaterial, movingMaterial, pixelMaterial);
    }

    [Test]
    public void PixelGridControls_AreVisibleOnlyInPixelMappingMode()
    {
        var selector = CreateSelector(out _, out var standardMaterial, out var movingMaterial, out var pixelMaterial, out _, out var pixelGridContainer, out _, out _);
        selector.SendMessage("Start");

        Assert.That(pixelGridContainer.activeSelf, Is.False);

        selector.SetMode(UI_FixtureModeSelector.FixtureMode.MovingHead);
        Assert.That(pixelGridContainer.activeSelf, Is.False);

        selector.SetMode(UI_FixtureModeSelector.FixtureMode.PixelMapping);
        Assert.That(pixelGridContainer.activeSelf, Is.True);

        selector.SetMode(UI_FixtureModeSelector.FixtureMode.Standard);
        Assert.That(pixelGridContainer.activeSelf, Is.False);

        DestroySelector(selector, standardMaterial, movingMaterial, pixelMaterial);
    }

    private static UI_FixtureModeSelector CreateSelector(
        out Renderer renderer,
        out Material standardMaterial,
        out Material movingMaterial,
        out Material pixelMaterial,
        out Text modeValueText,
        out GameObject pixelGridControlsContainer,
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

        var modeTextGo = new GameObject("mode-text");
        modeValueText = modeTextGo.AddComponent<Text>();

        pixelGridControlsContainer = new GameObject("pixel-grid-controls");

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
            .GetField("modeValueText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(selector, modeValueText);

        typeof(UI_FixtureModeSelector)
            .GetField("pixelGridControlsContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(selector, pixelGridControlsContainer);

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
