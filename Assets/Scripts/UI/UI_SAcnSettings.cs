using UnityEngine;
using UnityEngine.UI;

public class UI_SAcnSettings : MonoBehaviour
{
    [SerializeField] private Text transportModeText;
    [SerializeField] private Text multicastAddressText;
    [SerializeField] private Text unicastBindAddressText;
    [SerializeField] private Text listenPortText;

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
    }
}
