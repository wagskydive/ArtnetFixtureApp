using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;


public class UI_IpField : MonoBehaviour
{
    [SerializeField] private bool canEditOctet1;
    [SerializeField] Button SetOctet1Button;
    [SerializeField] Text SetOctet1ButtonText;


    [SerializeField] private bool canEditOctet2;
    [SerializeField] Button SetOctet2Button;
    [SerializeField] Text SetOctet2ButtonText;


    [SerializeField] private bool canEditOctet3;
    [SerializeField] Button SetOctet3Button;
    [SerializeField] Text SetOctet3ButtonText;


    [SerializeField] private bool canEditOctet4;
    [SerializeField] Button SetOctet4Button;
    [SerializeField] Text SetOctet4ButtonText;


    public ValueBinding binding;

    //Text ipStringText;

    private IPAddress ipAddress;

    public string IpString { get => ipAddress.ToString(); }

    public event Action<string> OnIpSet;



    void OnEnable()
    {
        if (binding != null)
        {
            SetIpFromBinding();
        }
        else
        {
            RedrawTexts();
        }
        SetButtonsInteractable();
    }

    public void SetIpFromBinding()
    {
        if (binding != null)
        {
            string bindingAddressString = binding.GetValue().ToString();
            ResetIpFromString(bindingAddressString);
        }
    }
    private void SetIpAddressOcted(int index, int octet)
    {
        int clampedOctet = Math.Clamp(octet, 0, 255);
        byte[] newBytes = (byte[])ipAddress.GetAddressBytes().Clone();
        newBytes[index] = (byte)clampedOctet;
        ipAddress = new IPAddress(newBytes);

        RedrawTexts();
        OnIpSet?.Invoke(ipAddress.ToString());
    }

    public void SetOcted1(int octet)
    {
        if (!canEditOctet1)
        {
            return;
        }
        SetIpAddressOcted(0, octet);
    }



    public void SetOcted2(int octet)
    {
        if (!canEditOctet2)
        {
            return;
        }
        SetIpAddressOcted(1, octet);
    }


    public void SetOcted3(int octet)
    {
        if (!canEditOctet3)
        {
            return;
        }
        SetIpAddressOcted(2, octet);
    }


    public void SetOcted4(int octet)
    {
        if (!canEditOctet4)
        {
            return;
        }
        SetIpAddressOcted(3, octet);
    }

    void RedrawTexts()
    {
        byte[] bytes = ipAddress.GetAddressBytes();

        SetOctet1ButtonText.text = bytes[0].ToString();
        SetOctet2ButtonText.text = bytes[1].ToString();
        SetOctet3ButtonText.text = bytes[2].ToString();
        SetOctet4ButtonText.text = bytes[3].ToString();
    }

    void SetButtonsInteractable()
    {
        SetOctet1Button.interactable = canEditOctet1;
        SetOctet2Button.interactable = canEditOctet2;
        SetOctet3Button.interactable = canEditOctet3;
        SetOctet4Button.interactable = canEditOctet4;
    }




    public void ResetIpFromString(string newIpString)
    {
        IPAddress address;
        if (TryParseIpv4(newIpString, out address))
        {
            ipAddress = address;
            RedrawTexts();
        }
    }

    private static bool TryParseIpv4(string value, out IPAddress address)
    {
        if (!IPAddress.TryParse(value, out address))
        {
            return false;
        }

        return address.AddressFamily == AddressFamily.InterNetwork;
    }

}
