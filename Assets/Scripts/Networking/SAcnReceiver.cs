using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class SAcnReceiver : MonoBehaviour, INetworkReceiver, IDmxSettingsConsumer
{
    public event Action NoDataReceivedRecently;
    public event Action DataReceivedAgain;

    public static event Action OnSAcnReceiverStarted;


    //public int Universe1Base {get => SaveLoadSettings.LoadInt(SaveLoadSettings.DmxUniverseKey,1); set => SaveLoadSettings.SaveInt(SaveLoadSettings.DmxUniverseKey, value); }


    //public int StartChannel = 1;

    public DmxBuffer DmxBuffer;
    public bool ReceiveNetworkData = false;


    [HideInInspector]
    public bool HasReceivedDataRecently = false;

    private bool _hasNoDataEventSent;

    public string ProtocolName => "sACN";

    DmxBuffer INetworkReceiver.DmxBuffer { get => DmxBuffer; set => DmxBuffer = value; }
    bool INetworkReceiver.ReceiveNetworkData { get => ReceiveNetworkData; set => ReceiveNetworkData = value; }
    bool INetworkReceiver.HasReceivedDataRecently => HasReceivedDataRecently;
    float INetworkReceiver.TimeoutSeconds { get => _settings.CurrentSAcnParameters.timeoutSeconds; }

    private UdpClient _udpClient;
    private Thread _receiveThread;
    private bool _running;
    private volatile bool _receivedPacketThisFrame;
    private float _lastPacketTime;
    private readonly byte[] _packetBuffer = new byte[512];
    private readonly object _stateLock = new object();
    private readonly Dictionary<int, UniverseState> _universeStates = new Dictionary<int, UniverseState>();
    private readonly HashSet<int> _joinedMulticastUniverses = new HashSet<int>();

    private AndroidJavaObject multicastLock;
    private DmxSettingsSnapshot _settings;

    public static SAcnReceiver Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    void OnEnable()
    {
        DmxSettingsBus.OnChanged += ApplyDmxSettings;
    }

    void OnDisable()
    {
        DmxSettingsBus.OnChanged -= ApplyDmxSettings;
        StopReceiver();
    }

    private void Start()
    {
        //SetUniverse(Universe1Base);
        //SetStartChannel(StartChannel);

        if (DmxBuffer == null)
        {
            DmxBuffer = new DmxBuffer();
        }

        if (ReceiveNetworkData)
        {
            StartReceiver();
        }
    }

    private void OnDestroy()
    {
        if (ReceiveNetworkData)
        {
            StopReceiver();
        }
    }

    private void Update()
    {
        if (DmxBuffer == null)
        {
            return;
        }

        DmxBuffer.SwapIfNewFrame();

        if (_receivedPacketThisFrame)
        {
            _lastPacketTime = Time.time;
            _receivedPacketThisFrame = false;
        }

        HasReceivedDataRecently = (Time.time - _lastPacketTime) <= _settings.CurrentSAcnParameters.timeoutSeconds;

        if (!HasReceivedDataRecently)
        {
            if (!_hasNoDataEventSent)
            {
                NoDataReceivedRecently?.Invoke();
                _hasNoDataEventSent = true;
            }
        }
        else if (_hasNoDataEventSent)
        {
            DataReceivedAgain?.Invoke();
            _hasNoDataEventSent = false;
        }
    }





    private void AcquireMulticastLock()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
    using (var wifiManager = activity.Call<AndroidJavaObject>("getSystemService", "wifi"))
    {
        multicastLock = wifiManager.Call<AndroidJavaObject>("createMulticastLock", "sACNLock");
        multicastLock.Call("acquire");
    }

    Debug.Log("[sACN] Multicast lock acquired");
#endif
    }

    private void ReleaseMulticastLock()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    if (multicastLock != null)
    {
        multicastLock.Call("release");
        multicastLock = null;
        Debug.Log("[sACN] Multicast lock released");
    }
#endif
    }

    public void RestartReceiver()
    {
        _running = false;
        StartReceiver();
    }

    public void StartReceiver()
    {
        if (_running) return;

        Debug.Log("[sACN] Starting receiver...");

        try
        {
            _udpClient = new UdpClient(AddressFamily.InterNetwork);

            AcquireMulticastLock();

            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udpClient.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, true);

            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, _settings.CurrentSAcnParameters.listenPort));

            Debug.Log($"[sACN] Bound to port {_settings.CurrentSAcnParameters.listenPort}");

            if (_settings.CurrentSAcnParameters.useMulticast)
            {
                JoinConfiguredMulticastGroups();
            }

            _running = true;

            _receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
            _receiveThread.Start();

            OnSAcnReceiverStarted?.Invoke();

            Debug.Log("[sACN] Receiver thread started");
        }
        catch (Exception e)
        {
            Debug.LogError($"[sACN] StartReceiver FAILED: {e}");
        }
    }
    public void StopReceiver()
    {
        if (!_running)
        {
            return;
        }

        _running = false;

        if (_udpClient != null)
        {
            _udpClient.Close();
            _udpClient = null;
        }

        if (_receiveThread != null && _receiveThread.IsAlive)
        {
            _receiveThread.Abort();
            _receiveThread = null;
        }
        ReleaseMulticastLock();
    }

    private void ReceiveLoop()
    {
        IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);

        Debug.Log("[sACN] Receive thread running");

        while (_running)
        {
            try
            {
                byte[] data = _udpClient.Receive(ref remoteEndpoint);

                if (!TryParseSacnPacket(data, out SacnPacketMetadata metadata))
                {
                    continue;
                }

                if (metadata.IsSynchronizationPacket)
                {
                    ApplyPendingSync(metadata.SyncUniverse == 0 ? metadata.Universe : metadata.SyncUniverse);
                    continue;
                }

                ProcessDataPacket(data, metadata);
                NetworkDebugService.Instance?.RecordPacket(ProtocolName, metadata.Universe, metadata.DmxLength, remoteEndpoint.ToString());

                _receivedPacketThisFrame = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[sACN] ReceiveLoop ERROR: {e}");
            }
        }

        Debug.Log("[sACN] Receive thread stopped");
    }

    private void ProcessDataPacket(byte[] data, SacnPacketMetadata metadata)
    {
        if (metadata.Universe <= 0)
        {
            return;
        }

        lock (_stateLock)
        {
            if (!_universeStates.TryGetValue(metadata.Universe, out UniverseState universeState))
            {
                universeState = new UniverseState();
                _universeStates[metadata.Universe] = universeState;
            }

            string sourceKey = BuildSourceKey(metadata.SourceCid);
            if (!universeState.SourceStates.TryGetValue(sourceKey, out SourceState sourceState))
            {
                sourceState = new SourceState();
                universeState.SourceStates[sourceKey] = sourceState;
            }

            int copyLength = Mathf.Clamp(metadata.DmxLength, 0, 512);
            Buffer.BlockCopy(data, metadata.DmxStartIndex, sourceState.CurrentFrame, 0, copyLength);
            sourceState.FrameLength = copyLength;
            sourceState.Priority = metadata.Priority;
            sourceState.LastSeenUtcTicks = DateTime.UtcNow.Ticks;
            sourceState.HasPendingSync = metadata.SyncUniverse > 0;
            sourceState.SyncUniverse = metadata.SyncUniverse;

            if (sourceState.HasPendingSync)
            {
                Buffer.BlockCopy(sourceState.CurrentFrame, 0, sourceState.PendingFrame, 0, copyLength);
                sourceState.PendingFrameLength = copyLength;
                return;
            }

            byte[] merged = BuildMergedFrameForUniverseLocked(metadata.Universe);

            if (metadata.Universe == _settings.Universe1Based && merged != null && DmxBuffer != null)
            {
                DmxBuffer.WriteFrame(merged, 512);
            }
        }
    }

    private static bool TryParseSacnPacket(byte[] data, out SacnPacketMetadata metadata)
    {
        metadata = default;

        if (data == null || data.Length < 126)
        {
            return false;
        }

        if (data[4] != 0x41 || data[5] != 0x53 || data[6] != 0x43 || data[7] != 0x2D || data[8] != 0x45 || data[9] != 0x31 || data[10] != 0x2E || data[11] != 0x31 || data[12] != 0x37)
        {
            return false;
        }

        if (data[125] != 0x00)
        {
            return false;
        }

        metadata.Universe = (data[113] << 8) | data[114];
        metadata.Priority = data[108];
        metadata.SyncUniverse = (data[109] << 8) | data[110];
        uint vector = (uint)(
    (data[40] << 24) |
    (data[41] << 16) |
    (data[42] << 8) |
    data[43]);

        // E1.31 vectors
        const uint VECTOR_E131_DATA_PACKET = 0x00000002;
        const uint VECTOR_E131_EXTENDED_SYNCHRONIZATION = 0x00000001;

        metadata.IsSynchronizationPacket = vector == VECTOR_E131_EXTENDED_SYNCHRONIZATION;
        int propertyValueCount = (data[123] << 8) | data[124];
        metadata.DmxLength = Mathf.Clamp(propertyValueCount - 1, 0, 512);
        metadata.DmxStartIndex = 126;

        if (metadata.IsSynchronizationPacket)
        {
            return true;
        }

        metadata.SourceCid = new byte[16];
        Buffer.BlockCopy(data, 22, metadata.SourceCid, 0, 16);

        return data.Length >= metadata.DmxStartIndex + metadata.DmxLength;
    }

    private static int ClampUniverse(int universe1BasedValue)
    {
        if (universe1BasedValue < 1 || universe1BasedValue > 63999)
        {
            Debug.LogWarning($"sACN universe {universe1BasedValue} is invalid. Clamping to 0-63999.");
        }

        return Mathf.Clamp(universe1BasedValue, 1, 63999);
    }

    private static int ClampStartChannel(int startChannel1Based)
    {
        if (startChannel1Based < 1 || startChannel1Based > 512)
        {
            Debug.LogWarning($"Start channel {startChannel1Based} is invalid. Clamping to 1-512.");
        }

        return Mathf.Clamp(startChannel1Based, 1, 512);
    }




    private static bool TryParseIpv4(string value, out IPAddress address)
    {
        if (!IPAddress.TryParse(value, out address))
        {
            return false;
        }

        return address.AddressFamily == AddressFamily.InterNetwork;
    }

    private static bool IsMulticast(IPAddress address)
    {
        byte firstOctet = address.GetAddressBytes()[0];
        return firstOctet >= 224 && firstOctet <= 239;
    }

    private IPAddress ResolveBindAddress()
    {
        if (!_settings.CurrentSAcnParameters.useMulticast && TryParseIpv4(_settings.CurrentSAcnParameters.unicastBindAddress, out IPAddress bindAddress))
        {
            return bindAddress;
        }

        return IPAddress.Any;
    }

    private void ClampMulticastSubscriptions()
    {
        if (_settings.CurrentSAcnParameters.multicastUniverseSubscriptions == null)
        {
            _settings.CurrentSAcnParameters.multicastUniverseSubscriptions = new List<int>();
            return;
        }

        for (int i = 0; i < _settings.CurrentSAcnParameters.multicastUniverseSubscriptions.Count; i++)
        {
            _settings.CurrentSAcnParameters.multicastUniverseSubscriptions[i] = ClampUniverse(_settings.CurrentSAcnParameters.multicastUniverseSubscriptions[i]);
        }
    }

    private void JoinConfiguredMulticastGroups()
    {
        _joinedMulticastUniverses.Clear();
        JoinUniverseMulticastGroup(_settings.Universe1Based);

        if (_settings.CurrentSAcnParameters.multicastUniverseSubscriptions == null)
        {
            return;
        }

        for (int i = 0; i < _settings.CurrentSAcnParameters.multicastUniverseSubscriptions.Count; i++)
        {
            JoinUniverseMulticastGroup(_settings.CurrentSAcnParameters.multicastUniverseSubscriptions[i]);
        }
    }



    private void JoinUniverseMulticastGroup(int universe1Based)
    {
        if (universe1Based < 1 || universe1Based > 63999 || _joinedMulticastUniverses.Contains(universe1Based))
            return;

        IPAddress multicast = BuildUniverseMulticastAddress(universe1Based);

        try
        {
            _udpClient.JoinMulticastGroup(multicast, IPAddress.Any);

            Debug.Log($"[sACN] Joined multicast {multicast} (Universe {universe1Based})");

            _joinedMulticastUniverses.Add(universe1Based);
        }
        catch (Exception e)
        {
            Debug.LogError($"[sACN] Failed to join multicast {multicast}: {e}");
        }
    }

    private static IPAddress BuildUniverseMulticastAddress(int universe1Based)
    {
        return IPAddress.Parse(SAcnParameters.BuildUniverseMulticastAddress(universe1Based));
    }

    private void ApplyPendingSync(int syncUniverse)
    {
        lock (_stateLock)
        {
            foreach (KeyValuePair<int, UniverseState> universePair in _universeStates)
            {
                UniverseState universeState = universePair.Value;
                foreach (KeyValuePair<string, SourceState> sourcePair in universeState.SourceStates)
                {
                    SourceState state = sourcePair.Value;
                    if (!state.HasPendingSync || state.SyncUniverse != syncUniverse)
                    {
                        continue;
                    }

                    Buffer.BlockCopy(state.PendingFrame, 0, state.CurrentFrame, 0, state.PendingFrameLength);
                    state.FrameLength = state.PendingFrameLength;
                    state.HasPendingSync = false;
                }
            }

            if (DmxBuffer != null && _universeStates.ContainsKey(_settings.Universe1Based))
            {
                byte[] merged = BuildMergedFrameForUniverseLocked(_settings.Universe1Based);
                if (merged != null)
                {
                    DmxBuffer.WriteFrame(merged, 512);
                }
            }
        }
    }

    private byte[] BuildMergedFrameForUniverseLocked(int universe1Based)
    {
        if (!_universeStates.TryGetValue(universe1Based, out UniverseState universeState) || universeState.SourceStates.Count == 0)
        {
            return null;
        }

        int highestPriority = int.MinValue;
        foreach (KeyValuePair<string, SourceState> entry in universeState.SourceStates)
        {
            highestPriority = Mathf.Max(highestPriority, entry.Value.Priority);
        }

        Array.Clear(universeState.MergedFrame, 0, universeState.MergedFrame.Length);

        SourceState ltpWinner = null;
        if (_settings.CurrentSAcnParameters.useLtpMerge)
        {
            long latestTicks = long.MinValue;
            foreach (KeyValuePair<string, SourceState> entry in universeState.SourceStates)
            {
                SourceState candidate = entry.Value;
                if (candidate.Priority != highestPriority)
                {
                    continue;
                }

                if (candidate.LastSeenUtcTicks > latestTicks)
                {
                    latestTicks = candidate.LastSeenUtcTicks;
                    ltpWinner = candidate;
                }
            }
        }

        foreach (KeyValuePair<string, SourceState> entry in universeState.SourceStates)
        {
            SourceState sourceState = entry.Value;
            if (sourceState.Priority != highestPriority)
            {
                continue;
            }

            if (_settings.CurrentSAcnParameters.useLtpMerge)
            {
                if (sourceState == ltpWinner)
                {
                    Buffer.BlockCopy(sourceState.CurrentFrame, 0, universeState.MergedFrame, 0, sourceState.FrameLength);
                }
                continue;
            }

            for (int channel = 0; channel < sourceState.FrameLength; channel++)
            {
                byte value = sourceState.CurrentFrame[channel];
                if (value > universeState.MergedFrame[channel])
                {
                    universeState.MergedFrame[channel] = value;
                }
            }
        }

        return universeState.MergedFrame;
    }

    private static string BuildSourceKey(byte[] cid)
    {
        return Convert.ToBase64String(cid);
    }

    public void ApplyDmxSettings(DmxSettingsSnapshot settings)
    {
        _settings = settings;
        RestartReceiver();
    }

    private struct SacnPacketMetadata
    {
        public int Universe;
        public int DmxStartIndex;
        public int DmxLength;
        public int Priority;
        public int SyncUniverse;
        public bool IsSynchronizationPacket;
        public byte[] SourceCid;
    }

    private sealed class UniverseState
    {
        public readonly Dictionary<string, SourceState> SourceStates = new Dictionary<string, SourceState>();
        public readonly byte[] MergedFrame = new byte[512];
    }

    private sealed class SourceState
    {
        public readonly byte[] CurrentFrame = new byte[512];
        public readonly byte[] PendingFrame = new byte[512];
        public int FrameLength;
        public int PendingFrameLength;
        public int Priority;
        public int SyncUniverse;
        public bool HasPendingSync;
        public long LastSeenUtcTicks;
    }
}
