using UnityEngine;
using UnityEngine.Video;

public class PatternMediaTextureController : MonoBehaviour
{
    [SerializeField] private Renderer outputRenderer;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Texture2D fallbackImage;

    private Material _outputMaterial;

    private static readonly int MediaTextureId = Shader.PropertyToID("_MediaTex");
    private static readonly int FallbackTextureId = Shader.PropertyToID("_FallbackTex");
    private static readonly int UseMediaTextureId = Shader.PropertyToID("_UseMediaTex");

    private void Awake()
    {
        if (outputRenderer != null)
        {
            _outputMaterial = outputRenderer.material;
        }

        if (_outputMaterial != null && fallbackImage != null)
        {
            _outputMaterial.SetTexture(FallbackTextureId, fallbackImage);
        }
    }

    private void Update()
    {
        if (_outputMaterial == null)
        {
            return;
        }

        Texture videoTexture = videoPlayer != null ? videoPlayer.texture : null;
        if (videoTexture != null)
        {
            _outputMaterial.SetTexture(MediaTextureId, videoTexture);
            _outputMaterial.SetFloat(UseMediaTextureId, 1f);
            return;
        }

        _outputMaterial.SetFloat(UseMediaTextureId, 0f);
    }
}
