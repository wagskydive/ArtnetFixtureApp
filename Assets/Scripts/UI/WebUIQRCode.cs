using UnityEngine;
using UnityEngine.UI;

public class WebUIQRCode : MonoBehaviour
{
    public RawImage qrImage;
    public QRCodeGenerator generator;

    void Start()
    {
        string ip = IpSolver.ResolveLocalIpv4Address();
        string url = $"http://{ip}:8080";

        qrImage.texture = generator.GenerateQRCode(url);
    }
}