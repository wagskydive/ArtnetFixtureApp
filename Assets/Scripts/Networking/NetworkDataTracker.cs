using System;
using UnityEngine;
public class NetworkDataTracker : MonoBehaviour
{
    [SerializeField] private float timeoutSeconds = 2f;
    [SerializeField] private float restoreDelaySeconds = 0.2f; // small buffer

    private bool _isLost;

    private float _lostTimer = 0f;

    void Update()
    {
        long ticks = NetworkDmxPacketsHeartbeat.LastPacketTicks;

        if (ticks <= 0)
            return;

        float secondsSinceLastPacket =
            (float)(DateTime.UtcNow.Ticks - ticks) / TimeSpan.TicksPerSecond;

        if (secondsSinceLastPacket > timeoutSeconds)
        {
            _lostTimer += Time.deltaTime;

            if (!_isLost && _lostTimer >= 0.1f) // small debounce
            {
                _isLost = true;
                RaiseNetworkLostEvent();
            }
        }
        else
        {
            _lostTimer = 0f;

            if (_isLost)
            {
                _isLost = false;
                RaiseNetworkRestoredEvent();
            }
        }
    }


    void RaiseNetworkLostEvent()
    {
        NetworkDataEvents.RaiseNetworkLostEvent();
    }

    void RaiseNetworkRestoredEvent()
    {
        NetworkDataEvents.RaiseNetworkRestoredEvent();
    }
}