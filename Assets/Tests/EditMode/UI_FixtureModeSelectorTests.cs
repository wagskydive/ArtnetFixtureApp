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
        PlayerPrefs.DeleteKey("dmx.fixture.count");
    }






  
    [Test]
    public void PixelGridSize_UsesEightStepAndClampsBetweenEightAndThirtyTwo()
    {
        var selector = CreateSelector(out var renderer, out var standardMaterial, out var movingMaterial, out var pixelMaterial, out _, out _, out var rowsText, out var columnsText, out _);
        selector.SendMessage("Start");
        selector.SetMode(DmxModeManager.FixtureMode.PixelMapping);

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
        var selector = CreateSelector(out _, out var standardMaterial, out var movingMaterial, out var pixelMaterial, out _, out var pixelGridContainer, out _, out _, out _);
        selector.SendMessage("Start");

        Assert.That(pixelGridContainer.activeSelf, Is.False);

        selector.SetMode(DmxModeManager.FixtureMode.MovingHead);
        Assert.That(pixelGridContainer.activeSelf, Is.False);

        selector.SetMode(DmxModeManager.FixtureMode.PixelMapping);
        Assert.That(pixelGridContainer.activeSelf, Is.True);

        selector.SetMode(DmxModeManager.FixtureMode.Standard);
        Assert.That(pixelGridContainer.activeSelf, Is.False);

        DestroySelector(selector, standardMaterial, movingMaterial, pixelMaterial);
    }

    [Test]
    public void FixtureCountControls_AreVisibleOnlyInStandardMode()
    {
        var selector = CreateSelector(out _, out var standardMaterial, out var movingMaterial, out var pixelMaterial, out _, out _, out _, out _, out var fixtureCountContainer);
        selector.SendMessage("Start");

        Assert.That(fixtureCountContainer.activeSelf, Is.True);

        selector.SetMode(DmxModeManager.FixtureMode.MovingHead);
        Assert.That(fixtureCountContainer.activeSelf, Is.False);

        selector.SetMode(DmxModeManager.FixtureMode.PixelMapping);
        Assert.That(fixtureCountContainer.activeSelf, Is.False);

        selector.SetMode(DmxModeManager.FixtureMode.Standard);
        Assert.That(fixtureCountContainer.activeSelf, Is.True);

        DestroySelector(selector, standardMaterial, movingMaterial, pixelMaterial);
    }

    [Test]
    public void NonStandardModes_ForceSingleFixtureInstance()
    {
        var managerGo = new GameObject("fixture-manager");
        var manager = managerGo.AddComponent<UI_FixtureMeshManager>();
        var template = GameObject.CreatePrimitive(PrimitiveType.Quad);
        template.name = "FixtureTemplate";
        var receiver = template.AddComponent<ArtNetReceiver>();
        receiver.ReceiveNetworkData = false;
        receiver.DmxBuffer = new DmxBuffer();

        typeof(UI_FixtureMeshManager)
            .GetField("primaryReceiver", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(manager, receiver);
        typeof(UI_FixtureMeshManager)
            .GetField("fixtureTemplate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(manager, template);

        manager.RebuildFixtures(4);
        Assert.That(manager.FixtureCount, Is.EqualTo(4));

        var selector = CreateSelector(out _, out var standardMaterial, out var movingMaterial, out var pixelMaterial, out _, out _, out _, out _, out _);
        typeof(UI_FixtureModeSelector)
            .GetField("fixtureMeshManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(selector, manager);

        selector.SetMode(DmxModeManager.FixtureMode.MovingHead);
        Assert.That(manager.FixtureCount, Is.EqualTo(1));

        manager.RebuildFixtures(3);
        selector.SetMode(DmxModeManager.FixtureMode.PixelMapping);
        Assert.That(manager.FixtureCount, Is.EqualTo(1));

        Object.DestroyImmediate(managerGo);
        Object.DestroyImmediate(template);
        DestroySelector(selector, standardMaterial, movingMaterial, pixelMaterial);
    }

    [Test]
    public void SwitchingBackToStandard_RestoresSavedFixtureCountPreference()
    {
        PlayerPrefs.SetInt("dmx.fixture.count", 5);

        var managerGo = new GameObject("fixture-manager");
        var manager = managerGo.AddComponent<UI_FixtureMeshManager>();
        var template = GameObject.CreatePrimitive(PrimitiveType.Quad);
        template.name = "FixtureTemplate";
        var receiver = template.AddComponent<ArtNetReceiver>();
        receiver.ReceiveNetworkData = false;
        receiver.DmxBuffer = new DmxBuffer();

        typeof(UI_FixtureMeshManager)
            .GetField("primaryReceiver", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(manager, receiver);
        typeof(UI_FixtureMeshManager)
            .GetField("fixtureTemplate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(manager, template);

        manager.SendMessage("Start");
        Assert.That(manager.FixtureCount, Is.EqualTo(5));

        var selector = CreateSelector(out _, out var standardMaterial, out var movingMaterial, out var pixelMaterial, out _, out _, out _, out _, out _);
        typeof(UI_FixtureModeSelector)
            .GetField("fixtureMeshManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(selector, manager);

        selector.SetMode(DmxModeManager.FixtureMode.MovingHead);
        Assert.That(manager.FixtureCount, Is.EqualTo(1));

        selector.SetMode(DmxModeManager.FixtureMode.Standard);
        Assert.That(manager.FixtureCount, Is.EqualTo(5));

        Object.DestroyImmediate(managerGo);
        Object.DestroyImmediate(template);
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
        out Text pixelColumnsValue,
        out GameObject fixtureCountControlsContainer)
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
        fixtureCountControlsContainer = new GameObject("fixture-count-controls");

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

        typeof(UI_FixtureModeSelector)
            .GetField("fixtureCountControlsContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(selector, fixtureCountControlsContainer);

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
