namespace ASiNet.Connector.Enums;
public enum RouterControllerResponse : short
{
    None = 0,
    Done = 100,
    /// <summary>
    /// Маршрутизатор по указанному имени не найден.
    /// </summary>
    RouterNotFound = 201,
    /// <summary>
    /// Метод маршрутизатора по указаному имени ненайден.
    /// </summary>
    MethodNotFound = 202,
    /// <summary>
    /// Параметры метода не соответствуют ожиданиям.
    /// </summary>
    InvalidMethodParameters = 300,
    /// <summary>
    /// не удалось создать класс маршрутизатора, возможно отсутствует конструктор по умолчанию или класс является статическим.
    /// </summary>
    FailedToCreateRouterInstance = 301,
    /// <summary>
    /// Не удалось преобразовать ответ в объект.
    /// </summary>
    DeserializeJsonRequestError = 401,
    /// <summary>
    /// Не удалось стерилизовать объект в json.
    /// </summary>
    SerializeResponseToJsonError = 402,
}
