using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

[Serializable]
public class SAcnParameters
{
    public bool UseMulticast = true;
    public string MulticastAddress { get => GetCurrentMulticastAddress(); }

    public string UnicastBindAddress = "0.0.0.0";
    public int ListenPort = 5568;
    public float TimeoutSeconds = 2f;
    public bool UseLtpMerge = false;
    public List<int> MulticastUniverseSubscriptions = new List<int>();
    public bool DebugPanelVisible = false;

    public SAcnParameters(bool useMulticast, string unicastBindAddress, int listenPort, float timeoutSeconds, bool useLtpMerge, List<int> multicastUniverseSubscriptions, bool debugPanelVisible)
    {
        UseMulticast = useMulticast;
        UnicastBindAddress = unicastBindAddress;
        ListenPort = listenPort;
        TimeoutSeconds = timeoutSeconds;
        UseLtpMerge = useLtpMerge;
        MulticastUniverseSubscriptions = multicastUniverseSubscriptions;
        DebugPanelVisible = debugPanelVisible;
    }

    public void Clamp()
    {
        ListenPort = Mathf.Clamp(ListenPort, 1, 65535);
        TimeoutSeconds = Mathf.Max(0.1f, TimeoutSeconds);


        if (!TryParseIpv4(UnicastBindAddress, out _))
        {
            UnicastBindAddress = "0.0.0.0";
        }

        if (MulticastUniverseSubscriptions == null)
        {
            MulticastUniverseSubscriptions = new List<int>();
            return;
        }

        for (int i = 0; i < MulticastUniverseSubscriptions.Count; i++)
        {
            MulticastUniverseSubscriptions[i] = Mathf.Clamp(MulticastUniverseSubscriptions[i], 0, 63999);
        }
    }

    public static string BuildUniverseMulticastAddress(int universe1Based)
    {
        int safeUniverse = Mathf.Clamp(universe1Based, 1, 64000);
        int hi = (safeUniverse >> 8) & 0xFF;
        int lo = safeUniverse & 0xFF;
        return $"239.255.{hi}.{lo}";
    }

    private string GetCurrentMulticastAddress()
    {
        return BuildUniverseMulticastAddress(DmxSettingsService.Instance.CurrentDmxSettings.Universe1Based);
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


    public static List<int> ParseUniverseList(string csv)
    {
        var values = new List<int>();
        if (string.IsNullOrWhiteSpace(csv))
        {
            return values;
        }

        string[] entries = csv.Split(',');
        for (int i = 0; i < entries.Length; i++)
        {
            if (!int.TryParse(entries[i].Trim(), out int universe1Based))
            {
                continue;
            }

            int value = Mathf.Clamp(universe1Based, 1, 64000) - 1;
            if (!values.Contains(value))
            {
                values.Add(value);
            }
        }

        return values;
    }

    public static string BuildUniverseListCsv(List<int> universes1BasedValues)
    {
        if (universes1BasedValues == null || universes1BasedValues.Count == 0)
        {
            return string.Empty;
        }

        string[] values = new string[universes1BasedValues.Count];
        for (int i = 0; i < universes1BasedValues.Count; i++)
        {
            values[i] = Mathf.Clamp(universes1BasedValues[i], 1, 63999).ToString();
        }

        return string.Join(",", values);
    }

    public static SAcnParameters Load()
    {
        SAcnParameters parameters = new SAcnParameters
        (
            SaveLoadSettings.LoadInt(SaveLoadSettings.SAcnUseMulticastKey, 1) == 1,
            SaveLoadSettings.LoadString(SaveLoadSettings.SAcnUnicastBindAddressKey, "0.0.0.0"),
            SaveLoadSettings.LoadInt(SaveLoadSettings.SAcnListenPortKey, 5568),
            SaveLoadSettings.LoadFloat(SaveLoadSettings.SAcnTimeoutSecondsKey, 2f),
            SaveLoadSettings.LoadInt(SaveLoadSettings.SAcnUseLtpMergeKey, true ? 1 : 0) == 1,
            SAcnParameters.ParseUniverseList(SaveLoadSettings.LoadString(SaveLoadSettings.SAcnMulticastUniversesKey, string.Empty)),
            SaveLoadSettings.LoadInt(SaveLoadSettings.SAcnDebugVisibleKey, 0) == 1
        );
        parameters.Clamp();
        return parameters;
    }





    public static SAcnParameters Clone(SAcnParameters p)
    {
        if (p == null)
        {
            return null;
        }
        SAcnParameters newParameters = new SAcnParameters(
            p.UseMulticast,
            p.UnicastBindAddress,
            p.ListenPort,
            p.TimeoutSeconds,
            p.UseLtpMerge,
            new List<int>(p.MulticastUniverseSubscriptions),
            p.DebugPanelVisible);

        return newParameters;
    }

    public static SAcnParameters Default()
    {
        SAcnParameters newParameters = new SAcnParameters
        (
            true,
            "0.0.0.0",
            5568,
            2f,
            true,
            new List<int>(),
            false
        );
        return newParameters;
    }
}
