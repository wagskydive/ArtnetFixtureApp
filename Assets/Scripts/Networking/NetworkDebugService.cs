using System;
using System.Collections.Generic;
using UnityEngine;

public class NetworkDebugService : MonoBehaviour
{
    [Serializable]
    public class NetworkDebugSnapshot
    {
        public bool enabled;
        public int packetsReceived;
        public int packetsPerSecond;
        public int activeUniverse;
        public string protocol = "Unknown";
        public string[] recentMessages = Array.Empty<string>();
    }

    private readonly Queue<string> _messages = new Queue<string>(16);
    private readonly Queue<float> _packetTimes = new Queue<float>(128);
    private readonly object _sync = new object();
    private int _packetsReceived;

    public static NetworkDebugService Instance { get; private set; }

    public bool DebugVisible
    {
        get => SaveLoadSettings.LoadInt(SaveLoadSettings.SAcnDebugVisibleKey, 0) == 1;
        set
        {
            SaveLoadSettings.SaveInt(SaveLoadSettings.SAcnDebugVisibleKey, value ? 1 : 0);
            SaveLoadSettings.SaveAndInvokeEvent();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RecordPacket(string protocol, int universe1Based, int channels, string source)
    {
        lock (_sync)
        {
            _packetsReceived++;
            _packetTimes.Enqueue(Time.realtimeSinceStartup);
            while (_packetTimes.Count > 0 && Time.realtimeSinceStartup - _packetTimes.Peek() > 1f)
            {
                _packetTimes.Dequeue();
            }

            string message = $"{DateTime.UtcNow:HH:mm:ss} {protocol} u:{universe1Based} ch:{channels} src:{source}";
            _messages.Enqueue(message);
            while (_messages.Count > 12)
            {
                _messages.Dequeue();
            }
        }
    }

    public NetworkDebugSnapshot BuildSnapshot()
    {
        var snapshot = new NetworkDebugSnapshot();
        INetworkReceiver receiver = NetworkingModeManager.Instance?.NetworkReceiver;
        snapshot.enabled = DebugVisible;
        snapshot.activeUniverse = DmxSettingsService.Instance.CurrentDmxSettings.Universe1Based;
        snapshot.protocol = receiver?.ProtocolName ?? "Unknown";

        lock (_sync)
        {
            while (_packetTimes.Count > 0 && Time.realtimeSinceStartup - _packetTimes.Peek() > 1f)
            {
                _packetTimes.Dequeue();
            }

            snapshot.packetsReceived = _packetsReceived;
            snapshot.packetsPerSecond = _packetTimes.Count;
            snapshot.recentMessages = _messages.ToArray();
        }

        return snapshot;
    }
}
