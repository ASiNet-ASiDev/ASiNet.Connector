using ASiNet.Connector.Enums;
using System.Reflection;
using System.Text.Json;

namespace ASiNet.Connector;
public class HandlersController : IDisposable
{
    public HandlersController()
    {
        _handlers = new();
    }

    private Dictionary<Route, Delegate> _handlers;
    /// <summary>
    /// Будет вызван если в контроллере произайдёт неизвестная ошибка.
    /// </summary>

    public event Action<Exception>? ThrownException;
    /// <summary>
    /// Добавить новый обработчик.
    /// </summary>
    /// <param name="path">Путь по которому ожидается ответ.</param>
    /// <param name="handler">Обработчик.</param>
    public HandlerControllerResult AddHandler(Route route, Delegate handler)
    {
        if (_handlers.TryAdd(route, handler))
            return HandlerControllerResult.Done;
        return HandlerControllerResult.AddHandlerFailed;
    }
    /// <summary>
    /// Отвязать существующий обработчик.
    /// </summary>
    /// <param name="handler">Обработчик.</param>
    public HandlerControllerResult RemoveHandler(Route route, Delegate handler)
    {
        if (_handlers.ContainsValue(handler) && _handlers.Remove(route))
            return HandlerControllerResult.Done;
        return HandlerControllerResult.RemoveHandlerNotFound;
    }

    /// <summary>
    /// Добавить новый обработчик.
    /// </summary>
    /// <param name="handler">Обработчик.</param>
    public HandlerControllerResult AddHandler(string path, Delegate handler)
    {
        if (_handlers.TryAdd(Route.FromPath(path), handler))
            return HandlerControllerResult.Done;
        return HandlerControllerResult.AddHandlerFailed;
    }
    /// <summary>
    /// Отвязать существующий обработчик.
    /// </summary>
    /// <param name="path">Путь по которому ожидается отет.</param>
    /// <param name="handler">Обработчик.</param>
    public HandlerControllerResult RemoveHandler(string path, Delegate handler)
    {
        if (_handlers.ContainsValue(handler) && _handlers.Remove(Route.FromPath(path)))
            return HandlerControllerResult.Done;
        return HandlerControllerResult.RemoveHandlerNotFound;
    }

    /// <summary>
    /// Вызвать обработчик.
    /// </summary>
    /// <param name="connection"> Подключение из которого вызывается обработчик.</param>
    /// <param name="pack">Пакет который спровоцировал вызов обработчика.</param>
    internal void ExecuteHandler(Connection connection, Package pack)
    {
        if (_handlers.TryGetValue(pack.Route, out var value))
        {
            var method = value.Method;
            var parameters = method.GetParameters();
            try
            {
                if (parameters.Length < 3)
                    throw new ArgumentException();

                if (parameters[0].ParameterType == typeof(Connection)
                && parameters[1].ParameterType == typeof(Package))
                {
                    var obj = JsonSerializer.Deserialize(pack.Json, parameters[2].ParameterType);
                    pack.HandlerControllerResult = HandlerControllerResult.Done;
                    method.Invoke(value.Target, new object?[] { connection, pack, obj });
                }
                else
                    throw new ArgumentException();
            }
            catch (JsonException)
            {
                pack.HandlerControllerResult = HandlerControllerResult.JsonDeserializeError;
                method.Invoke(value.Target, new object?[] { connection, pack, null });
            }
            catch (ArgumentException ex)
            {
                pack.HandlerControllerResult = HandlerControllerResult.InvalidHandlerParameters;
                if (parameters.Length == 3
                    && parameters[0].ParameterType == typeof(Connection)
                    && parameters[1].ParameterType == typeof(Package))
                    method.Invoke(value.Target, new object?[] { connection, pack, null });
                else
                    ThrownException?.Invoke(ex);
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                ThrownException?.Invoke(ex.InnerException);
            }
            catch (Exception ex)
            {
                ThrownException?.Invoke(ex);
            }
        }
    }

    public static HandlersController operator +(HandlersController rhc, (Route route, Delegate handler) obj)
    {
        rhc.AddHandler(obj.route, obj.handler);
        return rhc;
    }

    public static HandlersController operator -(HandlersController rhc, (Route route, Delegate handler) obj)
    {
        rhc.RemoveHandler(obj.route, obj.handler);
        return rhc;
    }

    public void Dispose()
    {
        ThrownException = null;
        _handlers.Clear();
    }
}
