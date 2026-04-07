using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UI_SAcnSettings : MonoBehaviour
{
    [SerializeField] private Text transportModeText;
    [SerializeField] private Text multicastAddressText;
    [SerializeField] private Text unicastBindAddressText;
    [SerializeField] private Text listenPortText;
    [SerializeField] private Text universeText;
    [SerializeField] private Text startChannelText;
    [SerializeField] private Text timeoutSecondsText;
    [SerializeField] private Text receiveNetworkDataText;
    [SerializeField] private Text mergeModeText;
    [SerializeField] private Text multicastUniversesText;

    private SAcnReceiver _sAcnReceiver;

    private void OnEnable()
    {
        LoadSAcnReceiver();
        RefreshLabels();
    }

    public void LoadSAcnReceiver()
    {
        _sAcnReceiver = NetworkingModeManager.Instance?.NetworkReceiver as SAcnReceiver;

        if (_sAcnReceiver == null)
        {
            gameObject.SetActive(false);
        }
    }

    public void SetMulticastMode()
    {
        SetUseMulticast(true);
    }

    public void SetUnicastMode()
    {
        SetUseMulticast(false);
    }

    public void SetUseMulticast(bool useMulticast)
    {
        if (_sAcnReceiver == null)
        {
            return;
        }

        _sAcnReceiver.SetTransportMode(useMulticast);
        RestartReceiverIfRunning();
        RefreshLabels();
    }

    public void SetMulticastAddress(string multicastAddress)
    {
        if (_sAcnReceiver == null || string.IsNullOrWhiteSpace(multicastAddress))
        {
            return;
        }

        _sAcnReceiver.SetMulticastAddressFromUserInput(multicastAddress.Trim());
        RestartReceiverIfRunning();
        RefreshLabels();
    }

    public void SetUnicastBindAddress(string bindAddress)
    {
        if (_sAcnReceiver == null || string.IsNullOrWhiteSpace(bindAddress))
        {
            return;
        }

        _sAcnReceiver.SetUnicastBindAddressFromUserInput(bindAddress.Trim());
        RestartReceiverIfRunning();
        RefreshLabels();
    }

    public void SetListenPort(int listenPort)
    {
        if (_sAcnReceiver == null)
        {
            return;
        }

        _sAcnReceiver.SetListenPortFromUserInput(listenPort);
        RestartReceiverIfRunning();
        RefreshLabels();
    }

    public void SetUniverse(int universe1Based)
    {
        if (_sAcnReceiver == null)
        {
            return;
        }

        _sAcnReceiver.SetUniverseFromUserInput(universe1Based);
        RestartReceiverIfRunning();
        RefreshLabels();
    }

    public void IncreaseUniverse()
    {
        SetUniverse((_sAcnReceiver?.GetUniverseForUserInput() ?? 1) + 1);
    }

    public void DecreaseUniverse()
    {
        SetUniverse((_sAcnReceiver?.GetUniverseForUserInput() ?? 1) - 1);
    }

    public void SetStartChannel(int startChannel1Based)
    {
        if (_sAcnReceiver == null)
        {
            return;
        }

        _sAcnReceiver.SetStartChannelFromUserInput(startChannel1Based);
        RefreshLabels();
    }

    public void IncreaseStartChannel()
    {
        SetStartChannel((_sAcnReceiver?.StartChannel ?? 1) + 1);
    }

    public void DecreaseStartChannel()
    {
        SetStartChannel((_sAcnReceiver?.StartChannel ?? 1) - 1);
    }

    public void SetTimeoutSeconds(float timeoutSeconds)
    {
        if (_sAcnReceiver == null)
        {
            return;
        }

        _sAcnReceiver.TimeoutSeconds = Mathf.Max(0.1f, timeoutSeconds);
        RefreshLabels();
    }

    public void SetReceiveNetworkData(bool receiveNetworkData)
    {
        if (_sAcnReceiver == null || _sAcnReceiver.ReceiveNetworkData == receiveNetworkData)
        {
            return;
        }

        _sAcnReceiver.ReceiveNetworkData = receiveNetworkData;
        if (receiveNetworkData)
        {
            _sAcnReceiver.StartReceiver();
        }
        else
        {
            _sAcnReceiver.StopReceiver();
        }

        RefreshLabels();
    }

    public void SetUseLtpMerge(bool useLtpMerge)
    {
        if (_sAcnReceiver == null)
        {
            return;
        }

        _sAcnReceiver.UseLtpMerge = useLtpMerge;
        RefreshLabels();
    }

    public void SetMulticastUniverseSubscriptionsCsv(string csvInput)
    {
        if (_sAcnReceiver == null)
        {
            return;
        }

        var parsedUniverses = new List<int>();

        if (!string.IsNullOrWhiteSpace(csvInput))
        {
            string[] parts = csvInput.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                if (!int.TryParse(parts[i].Trim(), out int universe1Based))
                {
                    continue;
                }

                int clamped = Mathf.Clamp(universe1Based, 1, 64000) - 1;
                if (!parsedUniverses.Contains(clamped))
                {
                    parsedUniverses.Add(clamped);
                }
            }
        }

        _sAcnReceiver.MulticastUniverseSubscriptions = parsedUniverses;
        RestartReceiverIfRunning();
        RefreshLabels();
    }

    public void IncreaseListenPort()
    {
        SetListenPort((_sAcnReceiver?.ListenPort ?? 5568) + 1);
    }

    public void DecreaseListenPort()
    {
        SetListenPort((_sAcnReceiver?.ListenPort ?? 5568) - 1);
    }

    private void RestartReceiverIfRunning()
    {
        if (_sAcnReceiver == null || !_sAcnReceiver.ReceiveNetworkData)
        {
            return;
        }

        _sAcnReceiver.StopReceiver();
        _sAcnReceiver.StartReceiver();
    }

    private void RefreshLabels()
    {
        if (_sAcnReceiver == null)
        {
            return;
        }

        if (transportModeText != null)
        {
            transportModeText.text = _sAcnReceiver.UseMulticast ? "Multicast" : "Unicast";
        }

        if (multicastAddressText != null)
        {
            multicastAddressText.text = _sAcnReceiver.MulticastAddress;
        }

        if (unicastBindAddressText != null)
        {
            unicastBindAddressText.text = _sAcnReceiver.UnicastBindAddress;
        }

        if (listenPortText != null)
        {
            listenPortText.text = _sAcnReceiver.ListenPort.ToString();
        }

        if (universeText != null)
        {
            universeText.text = _sAcnReceiver.GetUniverseForUserInput().ToString();
        }

        if (startChannelText != null)
        {
            startChannelText.text = _sAcnReceiver.StartChannel.ToString();
        }

        if (timeoutSecondsText != null)
        {
            timeoutSecondsText.text = _sAcnReceiver.TimeoutSeconds.ToString("0.0");
        }

        if (receiveNetworkDataText != null)
        {
            receiveNetworkDataText.text = _sAcnReceiver.ReceiveNetworkData ? "Enabled" : "Disabled";
        }

        if (mergeModeText != null)
        {
            mergeModeText.text = _sAcnReceiver.UseLtpMerge ? "LTP" : "HTP";
        }

        if (multicastUniversesText != null)
        {
            multicastUniversesText.text = BuildUniverseCsvForDisplay(_sAcnReceiver.MulticastUniverseSubscriptions);
        }
    }

    private static string BuildUniverseCsvForDisplay(List<int> universeList0Based)
    {
        if (universeList0Based == null || universeList0Based.Count == 0)
        {
            return string.Empty;
        }

        var values = new string[universeList0Based.Count];
        for (int i = 0; i < universeList0Based.Count; i++)
        {
            values[i] = (Mathf.Clamp(universeList0Based[i], 0, 63999) + 1).ToString();
        }

        return string.Join(",", values);
    }
}
