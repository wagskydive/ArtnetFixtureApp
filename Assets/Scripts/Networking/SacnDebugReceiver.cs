using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class SacnDebugReceiver : MonoBehaviour
{
    public int Port = 5568;

    [Tooltip("Leave empty for ANY. Set to your local IP if needed.")]
    public string LocalInterfaceIP = "";

    [Tooltip("Join sACN multicast (239.255.x.x)")]
    public bool UseMulticast = true;

    public int Universe = 1; // 1-based

    private UdpClient client;
    private Thread thread;
    private bool running;

    void Start()
    {
        try
        {
            client = new UdpClient(AddressFamily.InterNetwork);

            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, true);

            client.Client.Bind(new IPEndPoint(IPAddress.Any, Port));

            if (UseMulticast)
            {
                var multicastAddress = BuildSacnMulticast(Universe);

                IPAddress localIP = string.IsNullOrEmpty(LocalInterfaceIP)
                    ? IPAddress.Any
                    : IPAddress.Parse(LocalInterfaceIP);

                client.JoinMulticastGroup(multicastAddress, localIP);

                Debug.Log($"[sACN DEBUG] Joined multicast {multicastAddress} on {localIP}");
            }

            running = true;
            thread = new Thread(ReceiveLoop);
            thread.IsBackground = true;
            thread.Start();

            Debug.Log("[sACN DEBUG] Receiver started");
        }
        catch (Exception e)
        {
            Debug.LogError($"[sACN DEBUG] Init failed: {e}");
        }
    }

    void OnDestroy()
    {
        running = false;

        try { client?.Close(); } catch { }

        if (thread != null && thread.IsAlive)
            thread.Join();

        Debug.Log("[sACN DEBUG] Receiver stopped");
    }

    private void ReceiveLoop()
    {
        IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);

        while (running)
        {
            try
            {
                var data = client.Receive(ref remote);

                // Basic validation for sACN header
                bool looksLikeSacn = false;

                if (data.Length > 126 &&
                    data[4] == 0x41 && // 'A'
                    data[5] == 0x53 && // 'S'
                    data[6] == 0x43)   // 'C'
                {
                    looksLikeSacn = true;
                }

                Debug.Log($"[sACN DEBUG] Packet from {remote} | Size: {data.Length} | sACN: {looksLikeSacn}");
            }
            catch (Exception)
            {
                // ignore shutdown errors
            }
        }
    }

    private IPAddress BuildSacnMulticast(int universe)
    {
        int hi = (universe >> 8) & 0xFF;
        int lo = universe & 0xFF;
        return IPAddress.Parse($"239.255.{hi}.{lo}");
    }
}