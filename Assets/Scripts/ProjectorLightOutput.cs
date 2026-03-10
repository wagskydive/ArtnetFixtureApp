using UnityEngine;

public class ProjectorLightOutput : MonoBehaviour
{
    [SerializeField] private ArtNetReceiver artNetReceiver;
    [SerializeField] private Renderer outputRenderer;
    [SerializeField] private bool enableThermalProtection = true;
    [SerializeField][Range(0f, 1f)] private float thermalMinimumScale = 0.55f;
    [SerializeField][Range(0.001f, 0.1f)] private float thermalRampPerSecond = 0.02f;
    [SerializeField][Range(0.001f, 2f)] private float thermalRecoveryPerSecond = 0.15f;
    [SerializeField][Range(0f, 1f)] private float highLoadThreshold = 0.9f;

    private Material _outputMaterial;
    private Material _activeSharedMaterial;
    private float _thermalScale = 1f;

    private void Awake()
    {
        ResolveOutputMaterial();
        

    }

    private void Start()
    {
        DmxModeManager.OnModeChanged += HandleModeChange;
        isInMode = DmxModeManager.Instance.CurrentMode == DmxModeManager.FixtureMode.Standard;
 
    }

    void HandleModeChange(DmxModeManager.FixtureMode mode)
    {
        isInMode = mode == DmxModeManager.FixtureMode.Standard;
        
    }

    bool isInMode;

    void Update()
    {
        if (artNetReceiver == null || artNetReceiver.DmxBuffer == null || !ResolveOutputMaterial() || !isInMode)
        {
            return;
        }


        float dimmer = SurfaceProjectionDmxPersonality.ParseMasterDimmer(artNetReceiver);
        Color color = SurfaceProjectionDmxPersonality.ParseColor(artNetReceiver);
        float r = color.r;
        float g = color.g;
        float b = color.b;

        float thermalScale = enableThermalProtection
            ? ComputeThermalScale(dimmer, r, g, b)
            : 1f;

        _outputMaterial.SetColor("_Color", new Color(r, g, b, 1f));
        _outputMaterial.SetFloat("_Intensity", dimmer * thermalScale);
    }

    private bool ResolveOutputMaterial()
    {
        if (outputRenderer == null || outputRenderer.sharedMaterial == null)
        {
            return false;
        }

        if (_outputMaterial == null || _activeSharedMaterial != outputRenderer.sharedMaterial)
        {
            _activeSharedMaterial = outputRenderer.sharedMaterial;
            _outputMaterial = outputRenderer.material;
        }

        return _outputMaterial != null;
    }

    private float ComputeThermalScale(float dimmer, float r, float g, float b)
    {
        float projectedLoad = dimmer * Mathf.Max(r, Mathf.Max(g, b));
        bool isHighLoad = projectedLoad >= highLoadThreshold;
        float dt = Mathf.Max(Time.deltaTime, 1f / 30f);
        float ramp = thermalRampPerSecond * dt;
        float recovery = thermalRecoveryPerSecond * dt;

        if (isHighLoad)
        {
            _thermalScale = Mathf.Max(thermalMinimumScale, _thermalScale - ramp);
        }
        else
        {
            _thermalScale = Mathf.Min(1f, _thermalScale + recovery);
        }

        return _thermalScale;
    }
}
