using ASiNet.Connector.Enums;

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
    public required string RouterName { get; init; } = null!;
    /// <summary>
    /// Имя метода на который будет отправлен пакет/от которого был получен.
    /// </summary>
    public required string MethodName { get; init; } = null!;
    /// <summary>
    /// Сериализованые отправленный/полученный объект.
    /// </summary>
    public required string Json { get; init; } = null!;

    internal static Package ErrorRCResponse(RouterControllerResponse response, string routerName, string methodName) => new()
    {
        Type = PackageType.Response,
        RouterControllerResult = response,
        RouterName = routerName,
        MethodName = methodName,
        Json = string.Empty,
    };
}
