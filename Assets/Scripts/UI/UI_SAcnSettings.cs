using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class UI_SAcnSettings : MonoBehaviour
{
    [SerializeField] private Text transportModeText;
    [SerializeField] private UI_IpField multicastAddress;
    [SerializeField] private UI_IpField unicastBindAddress;
    [SerializeField] private Text listenPortText;
    [SerializeField] private Text universeText;
    [SerializeField] private Text startChannelText;
    [SerializeField] private Text timeoutSecondsText;
    [SerializeField] private Text receiveNetworkDataText;
    [SerializeField] private Text mergeModeText;
    [SerializeField] private Text multicastUniversesText;

    public string MulticastAddress { get => GetMulticastAddress(); }

    public string UnicastBindAddress { get => GetUnicastAddress(); }

    private string GetMulticastAddress()
    {
        if (_sAcnReceiver != null)
        {
            return _sAcnReceiver.MulticastAddress;
        }
        else
        {
            return "0.0.0.0";
        }
    }

    private string GetUnicastAddress()
    {
        if (_sAcnReceiver != null)
        {
            return _sAcnReceiver.UnicastBindAddress;
        }
        else
        {
            return "0.0.0.0";
        }
    }

    void Awake()
    {
        if(multicastAddress != null)
        {
            multicastAddress.OnIpSet += SetMulticastAddress;
        }
        if(unicastBindAddress != null)
        {
            unicastBindAddress.OnIpSet += SetUnicastBindAddress;
        }
    }

    private SAcnReceiver _sAcnReceiver;

    private void OnEnable()
    {
        LoadSAcnReceiver();
        RefreshLabels();
    }

    public void RestartSAcnReceiver()
    {
        if (_sAcnReceiver != null)
        {
            _sAcnReceiver.RestartReceiver();
        }
    }

    public void LoadSAcnReceiver()
    {
        _sAcnReceiver = NetworkingModeManager.Instance?.NetworkReceiver as SAcnReceiver;

        if (_sAcnReceiver == null)
        {
            gameObject.SetActive(false);
        }
    }

    public void ChangeTransportMode()
    {
        if (_sAcnReceiver != null)
        {
            if (_sAcnReceiver.UseMulticast)
            {
                SetUnicastMode();
            }
            else
            {
                SetMulticastMode();
            }
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

    public void SetListenPort(string listenPortText)
    {
        if (int.TryParse(listenPortText, out int result))
        {
            SetListenPort(result);
        }
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

    public void SetUniverse(string universe1BasedText)
    {
        if (int.TryParse(universe1BasedText, out int result))
        {
            SetUniverse(result);
        }
    }

    public void SetUniverse(int universe1Based)
    {
        if (_sAcnReceiver == null)
        {
            return;
        }

        UI_DmxSettings.Instance.CurrentDmxUniverse = universe1Based;


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

    public void SetStartChannel(string startChannel1BasedText)
    {
        if (int.TryParse(startChannel1BasedText, out int result))
        {
            SetStartChannel(result);
        }
    }

    public void SetStartChannel(int startChannel1Based)
    {
        if (_sAcnReceiver == null)
        {
            return;
        }
        UI_DmxSettings.Instance.CurrentDmxChannel = startChannel1Based;


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

    public void SetTimeoutSeconds(string timeoutSecondsText)
    {
        if (float.TryParse(timeoutSecondsText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result))
        {
            SetTimeoutSeconds(result);
        }
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

    public void ChangeReceiveNetworkData()
    {
        SetReceiveNetworkData(!_sAcnReceiver.ReceiveNetworkData);
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

    public void ChangeMergeMode()
    {
        SetUseLtpMerge(!_sAcnReceiver.UseLtpMerge);
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

        if (multicastAddress != null)
        {
            if (_sAcnReceiver.UseMulticast == true)
            {
                multicastAddress.transform.parent.gameObject.SetActive(true);
                multicastAddress.SetIpFromBinding();
                //multicastAddress.ResetIpFromString(_sAcnReceiver.MulticastAddress);
            }
            else
            {
                multicastAddress.transform.parent.gameObject.SetActive(false);
            }

        }

        if (unicastBindAddress != null)
        {
            if (_sAcnReceiver.UseMulticast == false)
            {
                unicastBindAddress.transform.parent.gameObject.SetActive(true);
                unicastBindAddress.SetIpFromBinding();
                //unicastBindAddress.ResetIpFromString(_sAcnReceiver.UnicastBindAddress);
            }
            else
            {
                unicastBindAddress.transform.parent.gameObject.SetActive(false);
            }

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
