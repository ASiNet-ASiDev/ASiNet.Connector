namespace ASiNet.Connector.Enums;
public enum PackageType : short
{
    None = 0,
    Package = 101,
    ErrorResponse = 201,
    DoneResponse = 202,
    CloseHandler = 301,
    CreateHandler = 302,
}
