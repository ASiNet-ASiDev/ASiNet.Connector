using ASiNet.Connector.Enums;
using System.Text.Json;

namespace ASiNet.Connector;
public class Package
{
    /// <summary>
    /// Тип пакета (Запрос или ответ)
    /// </summary>
    public required PackageType Type { get; init; }
    /// <summary>
    /// Ответ от маршрутизатора, если получили неожиданный результат, проверьте этот параметр на наличие ошибок.
    /// </summary>
    public RouterControllerResponse RouterControllerResult { get; set; }
    /// <summary>
    /// Ответ от контроллера обработчиков ответов, если получили неожиданный результат, проверьте этот параметр на наличие ошибок.
    /// </summary>
    public HandlerControllerResult HandlerControllerResult { get; set; }
    /// <summary>
    /// Имя роутера на который будет отправлен пакет/от которого был получен.
    /// </summary>
    public required Route Route { get; set; }
    public required string Json { get; init; } = null!;

    internal static Package ErrorRCResponse(RouterControllerResponse response, Route route) => new()
    {
        Type = PackageType.Response,
        RouterControllerResult = response,
        HandlerControllerResult = HandlerControllerResult.None,
        Route = route,
        Json = string.Empty,
    };

    internal static Package CreateRequest<Tobj>(Tobj obj, Route route) => new()
    {
        Type = PackageType.Request,
        RouterControllerResult = RouterControllerResponse.None, 
        HandlerControllerResult = HandlerControllerResult.None,
        Route = route,
        Json = JsonSerializer.Serialize(obj),
    };

    internal static Package CreateRequest(string json, Route route) => new()
    {
        Type = PackageType.Request,
        RouterControllerResult = RouterControllerResponse.None,
        HandlerControllerResult = HandlerControllerResult.None,
        Route = route,
        Json = json,
    };
}
