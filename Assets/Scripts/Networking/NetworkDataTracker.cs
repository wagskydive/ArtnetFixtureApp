using System;
using UnityEngine;
public class NetworkDataTracker : MonoBehaviour
{
    [SerializeField] private float timeoutSeconds = 2f;
    [SerializeField] private float restoreDelaySeconds = 0.2f;
    [SerializeField] private float lostDebounceSeconds = 0.1f;
    [SerializeField] private float modeSwitchGraceSeconds = 0.75f;

    private bool _isLost;

    private float _lostTimer = 0f;
    private float _restoreTimer = 0f;

    void Update()
    {
        long ticks = NetworkDmxPacketsHeartbeat.LastPacketTicks;
        long resetTicks = NetworkDmxPacketsHeartbeat.LastResetTicks;

        if (ticks <= 0 || resetTicks <= 0)
            return;

        float secondsSinceModeReset =
            (float)(DateTime.UtcNow.Ticks - resetTicks) / TimeSpan.TicksPerSecond;

        if (secondsSinceModeReset < modeSwitchGraceSeconds)
        {
            _lostTimer = 0f;
            _restoreTimer = 0f;
            return;
        }

        float secondsSinceLastPacket =
            (float)(DateTime.UtcNow.Ticks - ticks) / TimeSpan.TicksPerSecond;

        if (secondsSinceLastPacket > timeoutSeconds)
        {
            _lostTimer += Time.deltaTime;
            _restoreTimer = 0f;

            if (!_isLost && _lostTimer >= lostDebounceSeconds)
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
                _restoreTimer += Time.deltaTime;
                if (_restoreTimer >= restoreDelaySeconds)
                {
                    _isLost = false;
                    _restoreTimer = 0f;
                    RaiseNetworkRestoredEvent();
                }
            }
            else
            {
                _restoreTimer = 0f;
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
