using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;



public class ArtNetReceiver : MonoBehaviour, INetworkReceiver, IDmxSettingsConsumer
{


    //public int Universe1Base { get => SaveLoadSettings.LoadInt(SaveLoadSettings.DmxUniverseKey, 1); set => SaveLoadSettings.SaveInt(SaveLoadSettings.DmxUniverseKey, value); }

    //public int Universe0Base { get => Universe1Base - 1; }


    public DmxBuffer Buffer { get; set; }
    public bool ReceiveNetworkData = true;

    public string ProtocolName => "Art-Net";


    //int INetworkReceiver.Universe1Based { get => Universe1Base; set => Universe1Base = ClampUniverse(value); }

    //DmxBuffer INetworkReceiver.Buffer { get => DmxBuffer; set => DmxBuffer = value; }
    bool INetworkReceiver.ReceiveNetworkData { get => ReceiveNetworkData; set => ReceiveNetworkData = value; }

    private UdpClient _udpClient;
    private Thread _receiveThread;
    private volatile bool _running = false;
    [SerializeField] private float staleFrameRepublishSeconds = 0.25f;

    private byte[] _packetBuffer = new byte[1024]; // reused buffer
    private readonly byte[] _lastReceivedFrame = new byte[512];
    private readonly byte[] _republishFrame = new byte[512];
    private readonly object _frameCacheLock = new object();
    private volatile bool _hasReceivedFrame;
    private float _nextStaleRepublishTime;

    private DmxSettingsSnapshot _settings;

    public static ArtNetReceiver Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    void Start()
    {

        if (Buffer == null)
        {
            Buffer = new DmxBuffer();
        }
    }

    void OnEnable()
    {
        DmxSettingsBus.OnChanged += ApplyDmxSettings;
    }

    void OnDisable()
    {
        DmxSettingsBus.OnChanged -= ApplyDmxSettings;
    }


    void OnDestroy()
    {
        if (ReceiveNetworkData)
        {
            StopReceiver();
        }
    }

    void Update()
    {
        if (Buffer == null)
        {

            return;
        }

        if (Buffer.TrySwap(out var buffer))
        {
            var frame = new DmxFrame(buffer);
            DmxDataService.PushFrame(frame);
            _nextStaleRepublishTime = Time.unscaledTime + staleFrameRepublishSeconds;
            return;
        }

        if (!_hasReceivedFrame || Time.unscaledTime < _nextStaleRepublishTime)
        {
            return;
        }

        lock (_frameCacheLock)
        {
            System.Buffer.BlockCopy(_lastReceivedFrame, 0, _republishFrame, 0, 512);
        }

        DmxDataService.PushFrame(new DmxFrame(_republishFrame));
        _nextStaleRepublishTime = Time.unscaledTime + staleFrameRepublishSeconds;

    }


    public void StartReceiver()
    {
        if (_running)
        {
            return;
        }
        _hasReceivedFrame = false; // ❌ BAD currently missing
                                   // or even better:
        NetworkDmxPacketsHeartbeat.Initialize();

        _udpClient = new UdpClient(6454);
        _running = true;

        _receiveThread = new Thread(ReceiveLoop);
        _receiveThread.IsBackground = true;
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
        if (_settings.IsSAcnMode)
            return;

        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

        while (_running)
        {

            try
            {
                if (_settings.IsSAcnMode)
                {
                    _running = false;
                    return;
                }

                byte[] data = _udpClient.Receive(ref remoteEP);


                if (IsArtPollPacket(data))
                {
                    SendArtPollReply(remoteEP);
                }

                if (IsArtDmxPacket(data))
                {


                    int universe = data[14] | (data[15] << 8);
                    if (universe != _settings.Universe0Based) continue;

                    int declaredLength = (data[16] << 8) | data[17];

                    // Actual available payload in packet
                    int availableLength = data.Length - 18;

                    // Use the safe minimum
                    int length = Math.Min(Math.Min(declaredLength, availableLength), 512);

                    if (length <= 0)
                        return;
                    //UnityEngine.Debug.Log($"Packet size: {data.Length}, Declared DMX length: {declaredLength}");
                    Array.Clear(_packetBuffer, 0, 512); // 🔥 critical fix
                    System.Buffer.BlockCopy(data, 18, _packetBuffer, 0, length);


                    Buffer.WriteFrame(_packetBuffer, length);
                    NetworkDmxPacketsHeartbeat.NotifyPacketReceived();
                    CacheLastReceivedFrame(_packetBuffer, length);

                }
            }
            catch (Exception)
            {
                // Silent fail for stability on embedded device
            }
        }
    }

    private bool IsArtDmxPacket(byte[] data)
    {
        if (data.Length < 18) return false;

        // Check header "Art-Net"
        return data[0] == 'A' &&
               data[1] == 'r' &&
               data[2] == 't' &&
               data[3] == '-' &&
               data[4] == 'N' &&
               data[5] == 'e' &&
               data[6] == 't' &&
               data[7] == 0x00 &&
               data[8] == 0x00 &&
               data[9] == 0x50; // OpCode low/high for ArtDMX
    }

    private bool IsArtPollPacket(byte[] data)
    {
        if (data.Length < 10) return false;

        return data[0] == 'A' &&
               data[1] == 'r' &&
               data[2] == 't' &&
               data[3] == '-' &&
               data[4] == 'N' &&
               data[5] == 'e' &&
               data[6] == 't' &&
               data[7] == 0x00 &&
               data[8] == 0x00 &&
               data[9] == 0x20; // ArtPoll opcode
    }

    private void SendArtPollReply(IPEndPoint target)
    {
        byte[] reply = new byte[239]; // standard size

        // Header
        System.Text.Encoding.ASCII.GetBytes("Art-Net\0").CopyTo(reply, 0);

        // OpCode (ArtPollReply = 0x2100 little endian)
        reply[8] = 0x00;
        reply[9] = 0x21;

        // IP address (your device)
        byte[] ip = IpSolver.ResolveLocalIpv4AddressBytes();
        Array.Copy(ip, 0, reply, 10, 4);

        // Port (Art-Net port 6454)
        reply[14] = 0x36;
        reply[15] = 0x19;

        // Version info
        reply[16] = 0;
        reply[17] = 1;

        // Net/Subnet/Universe info
        reply[18] = 0; // Net
        reply[19] = (byte)_settings.Universe0Based;

        // Short name (18 bytes)
        WriteString(reply, 26, SaveLoadSettings.LoadString(SaveLoadSettings.DeviceNetworkKey, "DMX Projector"));

        // Long name (64 bytes)
        WriteString(reply, 44, SaveLoadSettings.LoadString(SaveLoadSettings.DeviceNetworkKey, "DMX Projector"));

        // Number of ports
        reply[173] = 0;
        reply[174] = 1;

        _udpClient.Send(reply, reply.Length, target);
    }

    private void WriteString(byte[] buffer, int index, string text)
    {
        var bytes = System.Text.Encoding.ASCII.GetBytes(text);
        Array.Copy(bytes, 0, buffer, index, Mathf.Min(bytes.Length, buffer.Length - index));
    }

    private static int ClampUniverse(int universe1Based)
    {
        if (universe1Based < 1 || universe1Based > 32769)
        {
            UnityEngine.Debug.LogWarning($"Universe {universe1Based} is invalid. Clamping to 0-15.");
        }

        return Mathf.Clamp(universe1Based, 1, 32769);
    }

    private static int ClampStartChannel(int startChannel1Based)
    {
        if (startChannel1Based < 1 || startChannel1Based > 512)
        {
            UnityEngine.Debug.LogWarning($"Start channel {startChannel1Based} is invalid. Clamping to 1-512.");
        }

        return Mathf.Clamp(startChannel1Based, 1, 512);
    }
    public void RestartReceiver()
    {
        StopReceiver();
        StartReceiver();
    }

    public void ApplyDmxSettings(DmxSettingsSnapshot settings)
    {
        _settings = settings;
        if (ReceiveNetworkData)
        {
            RestartReceiver();
        }
    }

    private void CacheLastReceivedFrame(byte[] source, int length)
    {
        lock (_frameCacheLock)
        {
            Array.Clear(_lastReceivedFrame, 0, _lastReceivedFrame.Length);
            int copyLength = Mathf.Clamp(length, 0, 512);
            System.Buffer.BlockCopy(source, 0, _lastReceivedFrame, 0, copyLength);
            _hasReceivedFrame = true;
        }
    }
}
