namespace ASiNet.Connector.Enums;
public enum HandlerControllerResponse
{
    None,
    Done,
    /// <summary>
    /// Не удалось добавить обработчик, возможно по этому пути уже есть привязанный разработчик, отвяжите его и попробуйте добавить снова.
    /// </summary>
    AddHandlerFailed,
    /// <summary>
    /// Нет привязанных обработчиков по такому пути.
    /// </summary>
    RemoveHandlerNotFound,
    /// <summary>
    /// Не удалось преобразовать полученный ответ в объект. Возможно вы указали неправильный тип, или json строка была повреждена.
    /// </summary>
    JsonDeserializeError,
    /// <summary>
    /// Неверные параметры обработчика, нормальные параметры: <see cref="Connection"/>, <see cref="Package"/>, Ожидаемый тип или <see cref="Object"/>
    /// </summary>
    InvalidHandlerParameters,

    HandlerTypeNotFound,

    /// <summary>
    /// Маршрутизатор по указанному имени не найден.
    /// </summary>
    HandlerNotFound = 201,
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
    FailedToCreateHandler = 301,
    /// <summary>
    /// Не удалось преобразовать ответ в объект.
    /// </summary>
    DeserializeJsonRequestError = 401,
    /// <summary>
    /// Не удалось стерилизовать объект в json.
    /// </summary>
    SerializeResponseToJsonError = 402,
}
