using System;

public interface INetworkReceiver
{
    event Action NoDataReceivedRecently;
    event Action DataReceivedAgain;


    DmxBuffer DmxBuffer { get; set; }
    bool ReceiveNetworkData { get; set; }
    bool HasReceivedDataRecently { get; }
    float TimeoutSeconds { get; }
    string ProtocolName { get; }

    void StartReceiver();
    void StopReceiver();
}
