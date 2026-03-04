using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;



public class ArtNetReceiver : MonoBehaviour
{
    public int Universe = 0;
    public DmxBuffer DmxBuffer;

    private UdpClient _udpClient;
    private Thread _receiveThread;
    private bool _running = false;

    private byte[] _packetBuffer = new byte[1024]; // reused buffer

    void Start()
    {
        DmxBuffer = new DmxBuffer();
        StartReceiver();
    }

    void OnDestroy()
    {
        StopReceiver();
    }

    void Update()
    {
        DmxBuffer.SwapIfNewFrame();
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
                    DmxBuffer.WriteFrame(_packetBuffer);
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
}


