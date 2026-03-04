using UnityEngine;

public class MasterDimmerController : MonoBehaviour
{
    [SerializeField] private DmxBuffer dmxBuffer;

    void Update()
    {
        // Simple dimmer control using channel 1 (brightness)
        float brightness = Mathf.PingPong(Time.time * 0.5f, 1.0f);
        
    }
}