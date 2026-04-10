using System;
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
    public float broadcastInterval = 2f;

    [Header("References")]
    public MultiDeviceLicenseManager licenseManager;

    private UdpClient sender;
    private UdpClient receiver;

    private string deviceId;
    private bool hasIAP;

    private float nextBroadcastTime;

    private void Start()
    {
        deviceId = SystemInfo.deviceUniqueIdentifier;

        // Setup sender
        sender = new UdpClient();
        sender.EnableBroadcast = true;

        // Setup receiver
        receiver = new UdpClient(port);
        receiver.BeginReceive(OnReceive, null);
    }

    private void Update()
    {

        if (Time.time >= nextBroadcastTime)
        {
            Broadcast();
            nextBroadcastTime = Time.time + broadcastInterval;
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
            string message = $"HEARTBEAT|{deviceId}|{(hasIAP ? 1 : 0)}";
            byte[] data = Encoding.UTF8.GetBytes(message);

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, port);
            sender.Send(data, data.Length, endPoint);
        }
        catch { }
    }

    private void OnReceive(IAsyncResult ar)
    {
        try
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
            byte[] data = receiver.EndReceive(ar, ref ep);

            string message = Encoding.UTF8.GetString(data);
            ParseMessage(message);

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
        Debug.Log("[MultiDevice] Heartbeat received from: "+remoteId+" Remote has ip is: "+remoteHasIAP);
        if (remoteId == deviceId)
        {
            licenseManager?.CleanupDevices();
            return;
        }
        licenseManager?.UpdateDevice(remoteId, remoteHasIAP);
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
        SendGoodbye();
        sender?.Close();
        receiver?.Close();
    }
}