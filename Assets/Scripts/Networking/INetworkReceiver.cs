using System;

public interface INetworkReceiver
{
    DmxBuffer DmxBuffer { get; set; }
    bool ReceiveNetworkData { get; set; }
    bool HasReceivedDataRecently { get; }
    float TimeoutSeconds { get; }
    string ProtocolName { get; }

    void StartReceiver();
    void StopReceiver();
}
