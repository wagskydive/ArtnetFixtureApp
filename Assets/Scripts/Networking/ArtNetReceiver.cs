using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;



public class ArtNetReceiver : MonoBehaviour, INetworkReceiver
{

    public event Action NoDataReceivedRecently;

    public event Action DataReceivedAgain;

    public int Universe1Base { get => SaveLoadSettings.LoadInt(SaveLoadSettings.DmxUniverseKey, 1); set => SaveLoadSettings.SaveInt(SaveLoadSettings.DmxUniverseKey, value); }

    public int Universe0Base { get => Universe1Base - 1; }

    [Range(1, 512)]
    public int StartChannel = 1;
    public DmxBuffer DmxBuffer;
    public bool ReceiveNetworkData = true;

    public string ProtocolName => "Art-Net";


    int INetworkReceiver.Universe1Based { get => Universe1Base; set => Universe1Base = ClampUniverse(value); }
    int INetworkReceiver.StartChannel { get => StartChannel; set => StartChannel = ClampStartChannel(value); }
    DmxBuffer INetworkReceiver.DmxBuffer { get => DmxBuffer; set => DmxBuffer = value; }
    bool INetworkReceiver.ReceiveNetworkData { get => ReceiveNetworkData; set => ReceiveNetworkData = value; }
    bool INetworkReceiver.HasReceivedDataRecently => HasReceivedDataRecently;
    float INetworkReceiver.TimeoutSeconds { get => TimeoutSeconds; set => TimeoutSeconds = Mathf.Max(0.1f, value); }

    private UdpClient _udpClient;
    private Thread _receiveThread;
    private bool _running = false;

    private byte[] _packetBuffer = new byte[1024]; // reused buffer

    [HideInInspector]
    public bool HasReceivedDataRecently = false;

    private volatile bool _receivedPacketThisFrame = false; // set by receive thread

    private float _lastPacketTime = 0f;
    public float TimeoutSeconds = 2f; // Show message if no data for 2 seconds

    bool HasNotReceivedDataEventSent = false;

    void Start()
    {
        SetUniverse(SaveLoadSettings.LoadInt(SaveLoadSettings.DmxUniverseKey, Universe0Base + 1));
        SetStartChannel(SaveLoadSettings.LoadInt(SaveLoadSettings.DmxChannelKey, StartChannel));
        if (DmxBuffer == null)
        {
            DmxBuffer = new DmxBuffer();
        }

        if (ReceiveNetworkData)
        {
            StartReceiver();
        }
    }

    public void SetUniverseFromUserInput(int universe1Based)
    {
        SetUniverse(universe1Based);
    }

    public void SetUniverse(int universe1Based)
    {
        Universe1Base = ClampUniverse(universe1Based);
    }



    public int GetUniverseForUserInput()
    {
        return Universe1Base;
    }

    public void SetStartChannelFromUserInput(int startChannel1Based)
    {
        SetStartChannel(startChannel1Based);
        SaveLoadSettings.SaveInt(SaveLoadSettings.DmxChannelKey, startChannel1Based);
    }

    public void SetStartChannel(int startChannel1Based)
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

    void OnDestroy()
    {
        if (ReceiveNetworkData)
        {
            StopReceiver();
        }
    }

    void Update()
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

        // Update whether we should show the "waiting for data" message
        HasReceivedDataRecently = (Time.time - _lastPacketTime) <= TimeoutSeconds;
        if (!HasReceivedDataRecently)
        {
            if (!HasNotReceivedDataEventSent)
            {
                NoDataReceivedRecently?.Invoke();
                HasNotReceivedDataEventSent = true;
            }
        }
        else
        {
            if (HasNotReceivedDataEventSent)
            {
                DataReceivedAgain?.Invoke();
                HasNotReceivedDataEventSent = false;
            }
        }
    }

    public void StartReceiver()
    {
        if (_running)
        {
            return;
        }

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
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

        while (_running)
        {
            try
            {
                byte[] data = _udpClient.Receive(ref remoteEP);

                if (IsArtPollPacket(data))
                {
                    SendArtPollReply(remoteEP);
                }

                if (IsArtDmxPacket(data))
                {
                    int universe = data[14] | (data[15] << 8);
                    if (universe != Universe0Base) continue;

                    int length = (data[16] << 8) | data[17];
                    if (length > 512) length = 512;

                    Buffer.BlockCopy(data, 18, _packetBuffer, 0, length);
                    DmxBuffer.WriteFrame(_packetBuffer, length);

                    _receivedPacketThisFrame = true;
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
        reply[19] = (byte)Universe0Base;

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
}
