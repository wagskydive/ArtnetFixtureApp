using System;

public interface INetworkReceiver
{
    static event Action OnPacketReceived;
    DmxBuffer Buffer { get; set; }
    bool ReceiveNetworkData { get; set; }
    string ProtocolName { get; }

    void StartReceiver();
    void StopReceiver();
}
