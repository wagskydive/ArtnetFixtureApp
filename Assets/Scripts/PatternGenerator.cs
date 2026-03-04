using UnityEngine;

public class PatternGenerator : MonoBehaviour
{
    [SerializeField] private DmxBuffer dmxBuffer;

    void Update()
    {
        // Simple color cycling pattern
        float t = Time.time * 0.1f;
        float r = Mathf.Sin(t) * 0.5f + 0.5f;
        float g = Mathf.Cos(t) * 0.5f + 0.5f;
        float b = 0.0f;

        
    }
}