namespace ASiNet.Connector.Enums;
public enum ConnectionStatus : short
{
    None = 0,
    Connected = 100,
    Disconnected = 200,
    ConnectionTimeout = 210,
    ConnectionCanceled = 211,
    ConnectionError = 212,
}
