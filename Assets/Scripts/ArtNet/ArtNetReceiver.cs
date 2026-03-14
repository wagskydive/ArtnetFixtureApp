using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;



public class ArtNetReceiver : MonoBehaviour
{

    public event Action NoDataReceivedRecently;

    public event Action DataReceivedAgain;

    [Range(0, 15)]
    public int Universe = 0;
    [Range(1, 512)]
    public int StartChannel = 1;
    public DmxBuffer DmxBuffer;
    public bool ReceiveNetworkData = true;

    private UdpClient _udpClient;
    private Thread _receiveThread;
    private bool _running = false;

    private byte[] _packetBuffer = new byte[1024]; // reused buffer

    [HideInInspector]
    public bool HasReceivedDataRecently = false;

    private volatile bool _receivedPacketThisFrame = false; // set by receive thread

    private float _lastPacketTime = 0f;
    public float TimeoutSeconds = 5f; // Show message if no data for 5 seconds

    bool HasNotReceivedDataEventSent = false;

    void Start()
    {
        Universe = ClampUniverse(Universe);
        StartChannel = ClampStartChannel(StartChannel);

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

    private void StartReceiver()
    {
        _udpClient = new UdpClient(6454);
        _running = true;

        _receiveThread = new Thread(ReceiveLoop);
        _receiveThread.IsBackground = true;
        _receiveThread.Start();
    }

    private void StopReceiver()
    {
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

                if (IsArtDmxPacket(data))
                {
                    int universe = data[14] | (data[15] << 8);
                    if (universe != Universe) continue;

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

    private static int ClampUniverse(int universe0Based)
    {
        if (universe0Based < 0 || universe0Based > 15)
        {
            UnityEngine.Debug.LogWarning($"Universe {universe0Based} is invalid. Clamping to 0-15.");
        }

        return Mathf.Clamp(universe0Based, 0, 15);
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
