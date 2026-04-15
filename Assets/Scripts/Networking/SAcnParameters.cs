using System;
using System.Collections.Generic;
using System.Net;
using UnityEditor.Experimental.GraphView;
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

    private static string BuildUniverseListCsv(List<int> universes1BasedValues)
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
        {
            useMulticast = SaveLoadSettings.LoadInt(SaveLoadSettings.SAcnUseMulticastKey, 1) == 1,
            multicastAddress = SaveLoadSettings.LoadString(SaveLoadSettings.SAcnMulticastAddressKey, "239.255.0.1"),
            unicastBindAddress = SaveLoadSettings.LoadString(SaveLoadSettings.SAcnUnicastBindAddressKey, "0.0.0.0"),
            listenPort = SaveLoadSettings.LoadInt(SaveLoadSettings.SAcnListenPortKey, 5568),
            timeoutSeconds = SaveLoadSettings.LoadFloat(SaveLoadSettings.SAcnTimeoutSecondsKey, 2f),
            useLtpMerge = SaveLoadSettings.LoadInt(SaveLoadSettings.SAcnUseLtpMergeKey, true ? 1 : 0) == 1,
            multicastUniverseSubscriptions = SAcnParameters.ParseUniverseList(SaveLoadSettings.LoadString(SaveLoadSettings.SAcnMulticastUniversesKey, string.Empty)),
            debugPanelVisible = SaveLoadSettings.LoadInt(SaveLoadSettings.SAcnDebugVisibleKey, 0) == 1
        };
        parameters.Clamp();
        return parameters;
    }

    public static event Action<SAcnParameters> OnSAcnParametersSaved;

    public static void Save(SAcnParameters p)
    {
        SaveLoadSettings.SaveInt(SaveLoadSettings.SAcnUseMulticastKey, p.useMulticast ? 1 : 0);
        SaveLoadSettings.SaveString(SaveLoadSettings.SAcnMulticastAddressKey, p.multicastAddress);
        SaveLoadSettings.SaveString(SaveLoadSettings.SAcnUnicastBindAddressKey, p.unicastBindAddress);
        SaveLoadSettings.SaveInt(SaveLoadSettings.SAcnListenPortKey, p.listenPort);
        SaveLoadSettings.SaveFloat(SaveLoadSettings.SAcnTimeoutSecondsKey, p.timeoutSeconds);
        SaveLoadSettings.SaveInt(SaveLoadSettings.SAcnUseLtpMergeKey, p.useLtpMerge ? 1 : 0);
        SaveLoadSettings.SaveString(SaveLoadSettings.SAcnMulticastUniversesKey, BuildUniverseListCsv(p.multicastUniverseSubscriptions));
        SaveLoadSettings.SaveInt(SaveLoadSettings.SAcnDebugVisibleKey, p.debugPanelVisible ? 1 : 0);
        OnSAcnParametersSaved?.Invoke(p);
    }

    public static SAcnParameters Clone(SAcnParameters p)
    {
        if (p == null)
        {
            return null;
        }
        SAcnParameters newParameters = new SAcnParameters
        {
            useMulticast = p.useMulticast,
            multicastAddress = p.multicastAddress,
            unicastBindAddress = p.unicastBindAddress,
            listenPort = p.listenPort,
            timeoutSeconds = p.timeoutSeconds,
            useLtpMerge = p.useLtpMerge,
            multicastUniverseSubscriptions = p.multicastUniverseSubscriptions,
            debugPanelVisible = p.debugPanelVisible
        };
        return newParameters;
    }

    public static SAcnParameters Default()
    {
        SAcnParameters newParameters = new SAcnParameters
        {
            useMulticast = true,
            multicastAddress = "239.255.0.1",
            unicastBindAddress = "0.0.0.0",
            listenPort = 5568,
            timeoutSeconds = 2f,
            useLtpMerge = true,
            multicastUniverseSubscriptions = new List<int>(),
            debugPanelVisible = false
        };
        return newParameters;
    }
}
