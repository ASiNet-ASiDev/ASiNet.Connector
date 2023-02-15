using ASiNet.Connector.Enums;
using System.Text.Json;

namespace ASiNet.Connector;
public class Package
{
    public PackageType Type { get; init; } = PackageType.Package;
    /// <summary>
    /// Ответ от маршрутизатора, если получили неожиданный результат, проверьте этот параметр на наличие ошибок.
    /// </summary>
    public HandlerControllerResponse Status { get; set; }
    /// <summary>
    /// Имя роутера на который будет отправлен пакет/от которого был получен.
    /// </summary>
    public required Route Route { get; set; }
    public required string Json { get; init; } = null!;

    internal static Package HandlerError(HandlerControllerResponse error, Route route) => new()
    {
        Type = PackageType.ErrorResponse,
        Status = error,
        Route = route,
        Json = string.Empty,
    };

    internal static Package HandlerDone(Route route) => new()
    {
        Type = PackageType.DoneResponse,
        Status = HandlerControllerResponse.Done,
        Route = route,
        Json = string.Empty,
    };

    internal static Package CreateRequest(string json, Route route) => new()
    {
        Status = HandlerControllerResponse.Done,
        Route = route,
        Json = json,
    };

    internal static Package CloseHandlerRequest(Route route) => new()
    {
        Type = PackageType.CloseHandler,
        Status = HandlerControllerResponse.Done,
        Route = route,
        Json = string.Empty,
    };

    internal static Package CreateHandlerRequest(Route route) => new()
    {
        Type = PackageType.CreateHandler,
        Status = HandlerControllerResponse.Done,
        Route = route,
        Json = string.Empty,
    };
}
