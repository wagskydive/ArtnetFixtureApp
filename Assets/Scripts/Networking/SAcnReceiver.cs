using System;
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

    private void Start()
    {
        Universe = ClampUniverse(Universe);
        StartChannel = ClampStartChannel(StartChannel);
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

        if (UseMulticast && TryParseIpv4(MulticastAddress, out IPAddress multicastIp) && IsMulticast(multicastIp))
        {
            _udpClient.JoinMulticastGroup(multicastIp);
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

                if (!TryParseSacnPacket(data, out int packetUniverse, out int dmxStartIndex, out int dmxLength))
                {
                    continue;
                }

                if (packetUniverse != Universe + 1)
                {
                    continue;
                }

                Buffer.BlockCopy(data, dmxStartIndex, _packetBuffer, 0, dmxLength);
                DmxBuffer.WriteFrame(_packetBuffer, dmxLength);
                _receivedPacketThisFrame = true;
            }
            catch (Exception)
            {
                // silent fail for embedded stability
            }
        }
    }

    private static bool TryParseSacnPacket(byte[] data, out int universe, out int dmxStartIndex, out int dmxLength)
    {
        universe = 0;
        dmxStartIndex = 0;
        dmxLength = 0;

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

        universe = (data[113] << 8) | data[114];
        int propertyValueCount = (data[123] << 8) | data[124];
        dmxLength = Mathf.Clamp(propertyValueCount - 1, 0, 512);
        dmxStartIndex = 126;

        return data.Length >= dmxStartIndex + dmxLength;
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
}
