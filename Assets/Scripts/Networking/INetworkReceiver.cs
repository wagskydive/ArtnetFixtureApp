using System;

public interface INetworkReceiver
{
    event Action NoDataReceivedRecently;
    event Action DataReceivedAgain;

    int Universe { get; set; }
    int StartChannel { get; set; }
    DmxBuffer DmxBuffer { get; set; }
    bool ReceiveNetworkData { get; set; }
    bool HasReceivedDataRecently { get; }
    float TimeoutSeconds { get; set; }
    string ProtocolName { get; }

    void SetUniverseFromUserInput(int universe1Based);
    void SetUniverse(int universe1Based);
    int GetUniverseForUserInput();
    void SetStartChannelFromUserInput(int startChannel1Based);
    void SetStartChannel(int startChannel1Based);
    int GetFixtureChannelValue(int relativeChannel);
    void StartReceiver();
    void StopReceiver();
}
