using UnityEngine;

public class ProjectorLightOutput : MonoBehaviour
{
    [SerializeField] private DmxBuffer dmxBuffer; // Reference to DmxBuffer

    void Update()
    {
        // Get RGB values from DMX channels (2 = Red, 3 = Green, 4 = Blue)
        byte r = dmxBuffer.GetChannel(2);
        byte g = dmxBuffer.GetChannel(3);
        byte b = dmxBuffer.GetChannel(4);

        // Apply to a shader (e.g., a simple unlit shader)
        Shader.SetGlobalFloat("_ColorR", r);
        Shader.SetGlobalFloat("_ColorG", g);
        Shader.SetGlobalFloat("_ColorB", b);
    }
}