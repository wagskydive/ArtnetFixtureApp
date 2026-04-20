using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

/// <summary>
/// Simple UDP heartbeat broadcaster + listener.
/// Sends device presence and receives others.
/// Plug output into MultiDeviceLicenseManager.
/// </summary>
public class NetworkHeartbeat : MonoBehaviour
{
    [Header("Network")]
    public int port = 7777;
    public float broadcastInterval = 1f;

    [Header("References")]
    public MultiDeviceLicenseManager licenseManager;

    private UdpClient sender;
    private UdpClient receiver;

    private string deviceId;
    private bool hasIAP;

    private float nextBroadcastTime;

    private readonly Queue<Action> mainThreadQueue = new Queue<Action>();

    private bool isActive = true;

    private long joinTimestamp;

    private void OnApplicationPause(bool pause)
    {
        isActive = !pause;

        if (pause)
        {
            StopNetworking();
        }
        else
        {
            StartNetworking();
        }
    }

    private void Start()
    {
        deviceId = SystemInfo.deviceUniqueIdentifier;

        // ✅ Use real-world time, not Time.time
        joinTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        StartNetworking();
    }

    private void StartNetworking()
    {
        if (sender != null || receiver != null)
            return;

        sender = new UdpClient();
        sender.EnableBroadcast = true;

        receiver = new UdpClient(port);
        receiver.BeginReceive(OnReceive, null);
    }

    private void Update()
    {
        if (!isActive)
            return;

        if (Time.time >= nextBroadcastTime)
        {
            Broadcast();
            nextBroadcastTime = Time.time + broadcastInterval;
        }
        while (mainThreadQueue.Count > 0)
        {
            mainThreadQueue.Dequeue().Invoke();
        }
    }

    public void SetLocalIAPState(bool ownsIAP)
    {
        hasIAP = ownsIAP;
    }

    private void Broadcast()
    {
        try
        {
            string message = $"HEARTBEAT|{deviceId}|{(hasIAP ? 1 : 0)}|{joinTimestamp}";
            byte[] data = Encoding.UTF8.GetBytes(message);

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, port);
            sender.Send(data, data.Length, endPoint);
        }
        catch { }
    }

    private void OnReceive(IAsyncResult ar)
    {
        if (receiver == null)
            return;
        try
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
            byte[] data = receiver.EndReceive(ar, ref ep);

            string message = Encoding.UTF8.GetString(data);
            mainThreadQueue.Enqueue(() => { ParseMessage(message); });


            if (receiver != null)
                receiver.BeginReceive(OnReceive, null);
        }
        catch { }
    }

    private void ParseMessage(string message)
    {
        var parts = message.Split('|');

        if (parts.Length == 0)
            return;

        string type = parts[0];

        if (type == "GOODBYE" && parts.Length >= 2)
        {
            string remoteId = parts[1];

            licenseManager?.ForceRemoveDevice(remoteId);
            return;
        }

        if (type == "HEARTBEAT" && parts.Length >= 3)
        {
            string remoteId = parts[1];
            bool remoteHasIAP = parts[2] == "1";
            long remoteJoinTime = long.Parse(parts[3]);


            Debug.Log("[MultiDevice] Heartbeat received from: " + remoteId + " Remote has ip is: " + remoteHasIAP);
            licenseManager?.UpdateDevice(remoteId, remoteHasIAP, remoteJoinTime);
        }
    }

    private void SendGoodbye()
    {
        try
        {
            string message = $"GOODBYE|{deviceId}";
            byte[] data = Encoding.UTF8.GetBytes(message);

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, port);
            sender.Send(data, data.Length, endPoint);
        }
        catch { }
    }

    private void OnApplicationQuit()
    {
        StopNetworking();
    }


    private void StopNetworking()
    {
        SendGoodbye();

        try { sender?.Close(); } catch { }
        try { receiver?.Close(); } catch { }

        sender = null;
        receiver = null;
    }
}