using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class SAcnReceiver : MonoBehaviour, INetworkReceiver
{
    public event Action NoDataReceivedRecently;
    public event Action DataReceivedAgain;

    [Range(0, 63999)]
    public int Universe = 0;

    [Range(1, 512)]
    public int StartChannel = 1;

    public DmxBuffer DmxBuffer;
    public bool ReceiveNetworkData = false;
    public float TimeoutSeconds = 2f;

    [Header("sACN Network")]
    public bool UseMulticast = true;
    public string MulticastAddress = "239.255.0.1";
    public string UnicastBindAddress = "0.0.0.0";
    [Range(1, 65535)]
    public int ListenPort = 5568;
    [Tooltip("Optional extra 0-based universes to listen to in addition to Universe.")]
    public List<int> MulticastUniverseSubscriptions = new List<int>();
    [Tooltip("When enabled, latest value wins after priority filtering. When disabled, highest value per channel wins (HTP).")]
    public bool UseLtpMerge = false;

    [HideInInspector]
    public bool HasReceivedDataRecently = false;

    private bool _hasNoDataEventSent;

    public string ProtocolName => "sACN";

    int INetworkReceiver.Universe { get => Universe; set => Universe = ClampUniverse(value); }
    int INetworkReceiver.StartChannel { get => StartChannel; set => StartChannel = ClampStartChannel(value); }
    DmxBuffer INetworkReceiver.DmxBuffer { get => DmxBuffer; set => DmxBuffer = value; }
    bool INetworkReceiver.ReceiveNetworkData { get => ReceiveNetworkData; set => ReceiveNetworkData = value; }
    bool INetworkReceiver.HasReceivedDataRecently => HasReceivedDataRecently;
    float INetworkReceiver.TimeoutSeconds { get => TimeoutSeconds; set => TimeoutSeconds = Mathf.Max(0.1f, value); }

    private UdpClient _udpClient;
    private Thread _receiveThread;
    private bool _running;
    private volatile bool _receivedPacketThisFrame;
    private float _lastPacketTime;
    private readonly byte[] _packetBuffer = new byte[512];
    private readonly object _stateLock = new object();
    private readonly Dictionary<int, UniverseState> _universeStates = new Dictionary<int, UniverseState>();
    private readonly HashSet<int> _joinedMulticastUniverses = new HashSet<int>();

    private void Start()
    {
        Universe = ClampUniverse(Universe);
        StartChannel = ClampStartChannel(StartChannel);
        ClampMulticastSubscriptions();
        LoadNetworkSettings();

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

        HasReceivedDataRecently = (Time.time - _lastPacketTime) <= TimeoutSeconds;

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

    public void SetUniverseFromUserInput(int universe1Based)
    {
        Universe = ClampUniverse(universe1Based - 1);
        ClampMulticastSubscriptions();
    }

    public int GetUniverseForUserInput()
    {
        return Universe + 1;
    }

    public void SetStartChannelFromUserInput(int startChannel1Based)
    {
        StartChannel = ClampStartChannel(startChannel1Based);
    }

    public int GetFixtureChannelValue(int relativeChannel)
    {
        if (DmxBuffer == null)
        {
            return 0;
        }

        int absoluteChannel = StartChannel + relativeChannel - 1;
        if (absoluteChannel < 1 || absoluteChannel > 512)
        {
            return 0;
        }

        return DmxBuffer.GetChannel1Based(absoluteChannel);
    }

    public void SetTransportMode(bool useMulticast)
    {
        UseMulticast = useMulticast;
        PersistNetworkSettings();
    }

    public void SetMulticastAddressFromUserInput(string multicastAddress)
    {
        if (!TryParseIpv4(multicastAddress, out IPAddress parsed) || !IsMulticast(parsed))
        {
            Debug.LogWarning($"Invalid multicast address: {multicastAddress}");
            return;
        }

        MulticastAddress = parsed.ToString();
        PersistNetworkSettings();
    }

    public void SetUnicastBindAddressFromUserInput(string bindAddress)
    {
        if (!TryParseIpv4(bindAddress, out IPAddress parsed))
        {
            Debug.LogWarning($"Invalid unicast bind address: {bindAddress}");
            return;
        }

        UnicastBindAddress = parsed.ToString();
        PersistNetworkSettings();
    }

    public void SetListenPortFromUserInput(int listenPort)
    {
        ListenPort = Mathf.Clamp(listenPort, 1, 65535);
        PersistNetworkSettings();
    }

    public void StartReceiver()
    {
        if (_running)
        {
            return;
        }

        IPAddress bindAddress = ResolveBindAddress();

        _udpClient = new UdpClient(AddressFamily.InterNetwork);
        _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _udpClient.Client.Bind(new IPEndPoint(bindAddress, ListenPort));

        if (UseMulticast)
        {
            JoinConfiguredMulticastGroups();
        }

        _running = true;

        _receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
        _receiveThread.Start();
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
    }

    private void ReceiveLoop()
    {
        IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);

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
                _receivedPacketThisFrame = true;
            }
            catch (Exception)
            {
                // silent fail for embedded stability
            }
        }
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
            if (metadata.Universe == Universe + 1 && merged != null && DmxBuffer != null)
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
        metadata.IsSynchronizationPacket = data[43] == 0x02;
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

    private static int ClampUniverse(int universe0Based)
    {
        if (universe0Based < 0 || universe0Based > 63999)
        {
            Debug.LogWarning($"sACN universe {universe0Based} is invalid. Clamping to 0-63999.");
        }

        return Mathf.Clamp(universe0Based, 0, 63999);
    }

    private static int ClampStartChannel(int startChannel1Based)
    {
        if (startChannel1Based < 1 || startChannel1Based > 512)
        {
            Debug.LogWarning($"Start channel {startChannel1Based} is invalid. Clamping to 1-512.");
        }

        return Mathf.Clamp(startChannel1Based, 1, 512);
    }

    private void LoadNetworkSettings()
    {
        UseMulticast = SaveLoadSettings.LoadInt(SaveLoadSettings.SAcnUseMulticastKey, 1) == 1;
        MulticastAddress = SaveLoadSettings.LoadString(SaveLoadSettings.SAcnMulticastAddressKey, MulticastAddress);
        UnicastBindAddress = SaveLoadSettings.LoadString(SaveLoadSettings.SAcnUnicastBindAddressKey, UnicastBindAddress);
        ListenPort = Mathf.Clamp(SaveLoadSettings.LoadInt(SaveLoadSettings.SAcnListenPortKey, ListenPort), 1, 65535);
    }

    private void PersistNetworkSettings()
    {
        SaveLoadSettings.SaveInt(SaveLoadSettings.SAcnUseMulticastKey, UseMulticast ? 1 : 0);
        SaveLoadSettings.SaveString(SaveLoadSettings.SAcnMulticastAddressKey, MulticastAddress);
        SaveLoadSettings.SaveString(SaveLoadSettings.SAcnUnicastBindAddressKey, UnicastBindAddress);
        SaveLoadSettings.SaveInt(SaveLoadSettings.SAcnListenPortKey, ListenPort);
        SaveLoadSettings.Save();
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
        if (!UseMulticast && TryParseIpv4(UnicastBindAddress, out IPAddress bindAddress))
        {
            return bindAddress;
        }

        return IPAddress.Any;
    }

    private void ClampMulticastSubscriptions()
    {
        if (MulticastUniverseSubscriptions == null)
        {
            MulticastUniverseSubscriptions = new List<int>();
            return;
        }

        for (int i = 0; i < MulticastUniverseSubscriptions.Count; i++)
        {
            MulticastUniverseSubscriptions[i] = ClampUniverse(MulticastUniverseSubscriptions[i]);
        }
    }

    private void JoinConfiguredMulticastGroups()
    {
        _joinedMulticastUniverses.Clear();
        JoinUniverseMulticastGroup(Universe + 1);

        if (MulticastUniverseSubscriptions == null)
        {
            return;
        }

        for (int i = 0; i < MulticastUniverseSubscriptions.Count; i++)
        {
            JoinUniverseMulticastGroup(MulticastUniverseSubscriptions[i] + 1);
        }
    }

    private void JoinUniverseMulticastGroup(int universe1Based)
    {
        if (universe1Based < 1 || universe1Based > 64000 || _joinedMulticastUniverses.Contains(universe1Based))
        {
            return;
        }

        IPAddress universeMulticast = BuildUniverseMulticastAddress(universe1Based);
        _udpClient.JoinMulticastGroup(universeMulticast);
        _joinedMulticastUniverses.Add(universe1Based);
    }

    private static IPAddress BuildUniverseMulticastAddress(int universe1Based)
    {
        int hi = (universe1Based >> 8) & 0xFF;
        int lo = universe1Based & 0xFF;
        return IPAddress.Parse($"239.255.{hi}.{lo}");
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

            if (DmxBuffer != null && _universeStates.ContainsKey(Universe + 1))
            {
                byte[] merged = BuildMergedFrameForUniverseLocked(Universe + 1);
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
        if (UseLtpMerge)
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

            if (UseLtpMerge)
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
