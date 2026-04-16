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
        if (DmxSettingsService.Instance != null)
        {
            return DmxSettingsService.Instance.CurrentDmxSettings.CurrentSAcnParameters.MulticastAddress;
        }
        else
        {
            return "0.0.0.0";
        }
    }

    private string GetUnicastAddress()
    {
        if (DmxSettingsService.Instance != null)
        {
            return DmxSettingsService.Instance.CurrentDmxSettings.CurrentSAcnParameters.UnicastBindAddress;
        }
        else
        {
            return "0.0.0.0";
        }
    }

    void Awake()
    {
        if (unicastBindAddress != null)
        {
            unicastBindAddress.OnIpSet += SetUnicastBindAddress;
        }
        SAcnReceiver.OnSAcnReceiverStarted += RefreshLabels;
    }

    private SAcnReceiver _sAcnReceiver;

    private void OnEnable()
    {
        LoadSAcnReceiver();
        RefreshLabels();
        DmxSettingsBus.OnChanged += HandleSettingsChange;
        SaveLoadSettings.OnSAcnParametersSaved += HandleParametersSaved;
    }

    private void HandleParametersSaved(SAcnParameters parameters)
    {
        HandleSettingsChange();
    }

    void OnDisable()
    {
        DmxSettingsBus.OnChanged -= HandleSettingsChange;
        SaveLoadSettings.OnSAcnParametersSaved -= HandleParametersSaved;

    }

    void HandleSettingsChange()
    {
        RestartReceiverIfRunning();
        RefreshLabels();
    }

    void HandleSettingsChange(DmxSettingsSnapshot snapshot)
    {
        HandleSettingsChange();
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
            //gameObject.SetActive(false);

        }
    }

    public void ChangeTransportMode()
    {
        if (DmxSettingsService.Instance != null)
        {
            if (DmxSettingsService.Instance.CurrentDmxSettings.CurrentSAcnParameters.UseMulticast)
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
        if (DmxSettingsService.Instance == null)
        {
            return;
        }

        SAcnParameters sAcnParameters = SAcnParameters.Clone(DmxSettingsService.Instance.CurrentDmxSettings.CurrentSAcnParameters);
        sAcnParameters.UseMulticast = useMulticast;
        SaveLoadSettings.SaveSAcnParameters(sAcnParameters);
        //DmxSettingsService.Instance.Save(new DmxSettingsSnapshot(sAcnParameters, DmxSettingsService.Instance.CurrentDmxSettings));
    }

    public void SetUnicastBindAddress(string bindAddress)
    {
        if (DmxSettingsService.Instance == null || string.IsNullOrWhiteSpace(bindAddress))
        {
            return;
        }

        SAcnParameters sAcnParameters = SAcnParameters.Clone(DmxSettingsService.Instance.CurrentDmxSettings.CurrentSAcnParameters);
        sAcnParameters.UnicastBindAddress = bindAddress;
        SaveLoadSettings.SaveSAcnParameters(sAcnParameters);
        //DmxSettingsService.Instance.Save(new DmxSettingsSnapshot(sAcnParameters, DmxSettingsService.Instance.CurrentDmxSettings));
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

        if (DmxSettingsService.Instance == null)
        {
            return;
        }

        SAcnParameters sAcnParameters = SAcnParameters.Clone(DmxSettingsService.Instance.CurrentDmxSettings.CurrentSAcnParameters);
        sAcnParameters.ListenPort = listenPort;
        SaveLoadSettings.SaveSAcnParameters(sAcnParameters);//new DmxSettingsSnapshot(sAcnParameters, DmxSettingsService.Instance.CurrentDmxSettings));

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
        if (DmxSettingsService.Instance == null)
        {
            return;
        }
        SaveLoadSettings.SaveDmxSettings(new DmxSettingsSnapshot(universe1Based, DmxSettingsService.Instance.CurrentDmxSettings));

        //DmxSettingsService.Instance.Save(new DmxSettingsSnapshot(universe1Based, DmxSettingsService.Instance.CurrentDmxSettings));
    }

    public void IncreaseUniverse()
    {
        if (DmxSettingsService.Instance == null)
        {
            return;
        }
        SetUniverse(DmxSettingsService.Instance.CurrentDmxSettings.Universe1Based + 1);
    }

    public void DecreaseUniverse()
    {
        if (DmxSettingsService.Instance == null)
        {
            return;
        }
        SetUniverse(DmxSettingsService.Instance.CurrentDmxSettings.Universe1Based - 1);
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
        if (DmxSettingsService.Instance == null)
        {
            return;
        }

        SaveLoadSettings.SaveDmxSettings(new DmxSettingsSnapshot(DmxSettingsService.Instance.CurrentDmxSettings, startChannel1Based));
    }

    public void IncreaseStartChannel()
    {
        if (DmxSettingsService.Instance == null)
        {
            return;
        }
        SetStartChannel(DmxSettingsService.Instance.CurrentDmxSettings.StartChannel + 1);
    }

    public void DecreaseStartChannel()
    {
        if (DmxSettingsService.Instance == null)
        {
            return;
        }
        SetStartChannel(DmxSettingsService.Instance.CurrentDmxSettings.StartChannel - 1);
    }

    public void SetTimeoutSeconds(string timeoutSecondsText)
    {
        if (DmxSettingsService.Instance == null)
        {
            return;
        }
        if (float.TryParse(timeoutSecondsText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result))
        {
            SetTimeoutSeconds(result);
        }
    }

    public void SetTimeoutSeconds(float timeoutSeconds)
    {
        if (DmxSettingsService.Instance == null)
        {
            return;
        }

        SAcnParameters sAcnParameters = SAcnParameters.Clone(DmxSettingsService.Instance.CurrentDmxSettings.CurrentSAcnParameters);
        sAcnParameters.TimeoutSeconds = timeoutSeconds;
        SaveLoadSettings.SaveSAcnParameters(sAcnParameters);
        //DmxSettingsService.Instance.Save(new DmxSettingsSnapshot(sAcnParameters, DmxSettingsService.Instance.CurrentDmxSettings));
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

    }

    public void ChangeMergeMode()
    {
        SetUseLtpMerge(!DmxSettingsService.Instance.CurrentDmxSettings.CurrentSAcnParameters.UseLtpMerge);


    }

    public void SetUseLtpMerge(bool useLtpMerge)
    {

        if (DmxSettingsService.Instance == null)
        {
            return;
        }

        SAcnParameters sAcnParameters = SAcnParameters.Clone(DmxSettingsService.Instance.CurrentDmxSettings.CurrentSAcnParameters);
        sAcnParameters.UseLtpMerge = useLtpMerge;
        SaveLoadSettings.SaveSAcnParameters(sAcnParameters);
        //DmxSettingsService.Instance.Save(new DmxSettingsSnapshot(sAcnParameters, DmxSettingsService.Instance.CurrentDmxSettings));
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

        if (DmxSettingsService.Instance == null)
        {
            return;
        }

        SAcnParameters sAcnParameters = SAcnParameters.Clone(DmxSettingsService.Instance.CurrentDmxSettings.CurrentSAcnParameters);
        sAcnParameters.MulticastUniverseSubscriptions = parsedUniverses;
        SaveLoadSettings.SaveSAcnParameters(sAcnParameters);
        //DmxSettingsService.Instance.Save(new DmxSettingsSnapshot(sAcnParameters, DmxSettingsService.Instance.CurrentDmxSettings));
    }

    public void IncreaseListenPort()
    {
        SetListenPort(DmxSettingsService.Instance.CurrentDmxSettings.CurrentSAcnParameters.ListenPort + 1);
    }

    public void DecreaseListenPort()
    {
        SetListenPort(Mathf.Clamp(DmxSettingsService.Instance.CurrentDmxSettings.CurrentSAcnParameters.ListenPort - 1, 0, 65535));
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
        if (DmxSettingsService.Instance == null)
        {
            return;
        }
        bool useMulticast = DmxSettingsService.Instance.CurrentDmxSettings.CurrentSAcnParameters.UseMulticast;

        if (transportModeText != null)
        {
            transportModeText.text = useMulticast ? "Multicast" : "Unicast";
        }

        if (multicastAddress != null)
        {
            if (useMulticast == true)
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
            if (useMulticast == false)
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
            listenPortText.text = DmxSettingsService.Instance.CurrentDmxSettings.CurrentSAcnParameters.ListenPort.ToString();
        }

        if (universeText != null)
        {
            universeText.text = DmxSettingsService.Instance.CurrentDmxSettings.Universe1Based.ToString();
        }

        if (startChannelText != null)
        {
            startChannelText.text = DmxSettingsService.Instance.CurrentDmxSettings.StartChannel.ToString();
        }

        if (timeoutSecondsText != null)
        {
            timeoutSecondsText.text = DmxSettingsService.Instance.CurrentDmxSettings.CurrentSAcnParameters.TimeoutSeconds.ToString("0.0");
        }

        if (receiveNetworkDataText != null)
        {
            if (_sAcnReceiver == null)
            {
                LoadSAcnReceiver();
            }
            if (_sAcnReceiver != null)
            {
                receiveNetworkDataText.text = _sAcnReceiver.ReceiveNetworkData ? "Enabled" : "Disabled";
            }

        }

        if (mergeModeText != null)
        {
            mergeModeText.text = DmxSettingsService.Instance.CurrentDmxSettings.CurrentSAcnParameters.UseLtpMerge ? "LTP" : "HTP";
        }

        if (multicastUniversesText != null)
        {
            multicastUniversesText.text = BuildUniverseCsvForDisplay(DmxSettingsService.Instance.CurrentDmxSettings.CurrentSAcnParameters.MulticastUniverseSubscriptions);
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

    private static string BuildUniverseSeperateLines(List<int> universeList0Based)
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

        return string.Join("/n", values);
    }
}
