using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

[Serializable]
public class SAcnParameters
{
    public bool useMulticast = true;
    public string multicastAddress = "239.255.0.1";
    public string unicastBindAddress = "0.0.0.0";
    public int listenPort = 5568;
    public float timeoutSeconds = 2f;
    public bool useLtpMerge = false;
    public List<int> multicastUniverseSubscriptions = new List<int>();
    public bool debugPanelVisible = false;

    public void Clamp()
    {
        listenPort = Mathf.Clamp(listenPort, 1, 65535);
        timeoutSeconds = Mathf.Max(0.1f, timeoutSeconds);

        if (!TryParseIpv4(multicastAddress, out IPAddress multicast) || !IsMulticast(multicast))
        {
            multicastAddress = "239.255.0.1";
        }

        if (!TryParseIpv4(unicastBindAddress, out _))
        {
            unicastBindAddress = "0.0.0.0";
        }

        if (multicastUniverseSubscriptions == null)
        {
            multicastUniverseSubscriptions = new List<int>();
            return;
        }

        for (int i = 0; i < multicastUniverseSubscriptions.Count; i++)
        {
            multicastUniverseSubscriptions[i] = Mathf.Clamp(multicastUniverseSubscriptions[i], 0, 63999);
        }
    }

    public static string BuildUniverseMulticastAddress(int universe1Based)
    {
        int safeUniverse = Mathf.Clamp(universe1Based, 1, 64000);
        int hi = (safeUniverse >> 8) & 0xFF;
        int lo = safeUniverse & 0xFF;
        return $"239.255.{hi}.{lo}";
    }

    public static bool TryParseUniverseFromMulticast(string address, out int universe1Based)
    {
        universe1Based = 1;
        if (!TryParseIpv4(address, out IPAddress parsed) || !IsMulticast(parsed))
        {
            return false;
        }

        byte[] octets = parsed.GetAddressBytes();
        if (octets.Length != 4 || octets[0] != 239 || octets[1] != 255)
        {
            return false;
        }

        universe1Based = (octets[2] << 8) | octets[3];
        return universe1Based >= 1 && universe1Based <= 64000;
    }

    private static bool TryParseIpv4(string value, out IPAddress address)
    {
        if (!IPAddress.TryParse(value, out address))
        {
            return false;
        }

        return address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
    }

    private static bool IsMulticast(IPAddress address)
    {
        byte firstOctet = address.GetAddressBytes()[0];
        return firstOctet >= 224 && firstOctet <= 239;
    }
}
