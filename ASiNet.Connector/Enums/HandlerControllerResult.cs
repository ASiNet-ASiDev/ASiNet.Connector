namespace ASiNet.Connector.Enums;
public enum HandlerControllerResult
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
}
