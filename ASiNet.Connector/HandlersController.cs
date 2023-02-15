using ASiNet.Connector.Attributes;
using ASiNet.Connector.Enums;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text.Json;

namespace ASiNet.Connector;
public class HandlersController : IDisposable
{
    public HandlersController()
    {
        _activeHandlers = new();
        _handlers = new();
    }

    private List<Type> _activeHandlers;
    /// <summary>
    /// Будет вызван если в контроллере произайдёт неизвестная ошибка.
    /// </summary>

    private List<(Route route, object obj)> _handlers;

    public event Action<Exception>? ThrownException;
    /// <summary>
    /// Добавить новый обработчик.
    /// </summary>
    /// <param name="path">Путь по которому ожидается ответ.</param>
    /// <param name="handler">Обработчик.</param>
    public HandlerControllerResponse AddHandler(Type handler)
    {
        _activeHandlers.Add(handler);
        return HandlerControllerResponse.Done;
    }
    /// <summary>
    /// Отвязать существующий обработчик.
    /// </summary>
    /// <param name="handler">Обработчик.</param>
    public HandlerControllerResponse RemoveHandler(Type handler)
    {
        if (_activeHandlers.Remove(handler))
            return HandlerControllerResponse.Done;
        return HandlerControllerResponse.RemoveHandlerNotFound;
    }

    public void CloseHandler(Connection connection, Route path)
    {
        if (_handlers.FirstOrDefault(x => x.route.id == path.id && x.route.ControllerName == path.ControllerName) is (Route route, object obj) handler)
        {
            var dispose = handler.obj as IDisposable;
            dispose?.Dispose();
            connection.Send(Package.CloseHandlerRequest(path));
        }
    }

    public void CloseHandler(Connection connection, object handler)
    {
        if (_handlers.FirstOrDefault(x => x.obj.Equals(handler)) is (Route route, object obj) handl)
        {
            var dispose = handl.obj as IDisposable;
            dispose?.Dispose();
            connection.Send(Package.CloseHandlerRequest(handl.route));
            _handlers.Remove(handl);
        }
    }

    internal Package CloseHandlerRequest(Route path)
    {
        if (_handlers.FirstOrDefault(x => x.route.id == path.id && x.route.ControllerName == path.ControllerName) is (Route route, object obj) handler)
        {
            var dispose = handler.obj as IDisposable;
            dispose?.Dispose();
            dispose?.Dispose();
            _handlers.Remove(handler);

            return Package.HandlerDone(path);
        }
        else
            return Package.HandlerError(HandlerControllerResponse.HandlerNotFound, path);
    }

    public (HandlerControllerResponse response, Route route) CreateHandler(Connection connection, Route path)
    {
        try
        {
            var handler = _activeHandlers.FirstOrDefault(x => x.GetCustomAttribute<HandlerAttribute>() is HandlerAttribute attr && attr.Name == path.ControllerName);
            if (handler is null)
                return (HandlerControllerResponse.HandlerTypeNotFound, path);
            var id = Random.Shared.Next(10000000, 99999999);
            var route = path with { id = id, };

            var instance = Activator.CreateInstance(handler);
            if (instance is null)
                return (HandlerControllerResponse.FailedToCreateHandler, path);

            _handlers.Add((route, instance));

            connection.Send(Package.CreateHandlerRequest(route));

            return (HandlerControllerResponse.Done, route);
        }
        catch (Exception)
        {
            return (HandlerControllerResponse.FailedToCreateHandler, path);
        }
    }

    internal Package CreateHandler(Route path)
    {
        try
        {
            var handler = _activeHandlers.FirstOrDefault(x => (x.GetCustomAttribute<HandlerAttribute>()) is HandlerAttribute attr && attr.Name == path.ControllerName);
            if (handler is null)
                return Package.HandlerError(HandlerControllerResponse.HandlerTypeNotFound, path);
            var id = Random.Shared.Next(10000000, 99999999);
            var route = path with { id = id, };

            var instance = Activator.CreateInstance(handler);
            if (instance is null)
                return Package.HandlerError(HandlerControllerResponse.FailedToCreateHandler, path);

            _handlers.Add((route, instance));

            return Package.CreateHandlerRequest(route);
        }
        catch (Exception)
        {
            return Package.HandlerError(HandlerControllerResponse.FailedToCreateHandler, path);
        }
    }

    internal Package CreateHandlerRequest(Route path)
    {
        var result = CreateConnectedHandler(path);
        if(result.err == HandlerControllerResponse.Done)
            return Package.HandlerDone(result.route);
        return Package.HandlerError(result.err, result.route);
    }

    internal (HandlerControllerResponse err, Route route) CreateConnectedHandler(Route path)
    {
        try
        {
            var handler = _activeHandlers.FirstOrDefault(x => x.GetCustomAttribute<HandlerAttribute>() is HandlerAttribute attr && attr.Name == path.ControllerName);
            if (handler is null)
                return (HandlerControllerResponse.HandlerTypeNotFound, Route.Default);
            var route = path;

            var instance = Activator.CreateInstance(handler);
            if (instance is null)
                return (HandlerControllerResponse.FailedToCreateHandler, Route.Default);

            _handlers.Add((route, instance));

            return (HandlerControllerResponse.Done, route);
        }
        catch (Exception)
        {
            return (HandlerControllerResponse.FailedToCreateHandler, Route.Default);
        }
    }

    internal Package? RouteToHandler(Connection connection, Package package)
    {
        try
        {
            if (_handlers.FirstOrDefault(x => x.route.id == package.Route.id && x.route.ControllerName == package.Route.ControllerName) is (Route route, object obj) handler)
                return ExecuteHandlerMethod(connection, handler.obj, package);
            else
                return Package.HandlerError(HandlerControllerResponse.HandlerNotFound, package.Route);
            
        }
        catch (Exception)
        {
            return Package.HandlerError(HandlerControllerResponse.HandlerNotFound, package.Route);
        }
    }

    private Package? ExecuteHandlerMethod(Connection connection, object handler, Package package)
    {
        var type = handler.GetType();
        MethodInfo? method = type.GetMethods().FirstOrDefault(x => x.GetCustomAttribute<HandlerMethodAttribute>() is HandlerMethodAttribute attr
                                                                        && attr.Name == package.Route.MethodName);
        if (method is null)
            return Package.HandlerError(HandlerControllerResponse.MethodNotFound, package.Route);

        var parameters = method.GetParameters();
        if (parameters.Length == 2 && parameters[0].ParameterType == typeof(Connection))
        {
            var param = DeserializeToParameter(package.Json, parameters[1].ParameterType);
            if (param is null)
                return Package.HandlerError(HandlerControllerResponse.DeserializeJsonRequestError, package.Route);
            var result = GetJsonResponse(connection, param, handler, method);

            if (result.json == string.Empty)
                return null;
            else if (result.json is null)
                return Package.HandlerError(HandlerControllerResponse.SerializeResponseToJsonError, package.Route);
            var packageResult = new Package()
            {
                Status = HandlerControllerResponse.Done,
                Json = result.json,
                Route = package.Route with { MethodName = result.route.MethodName },
            };
            return packageResult;
        }
        else
            return Package.HandlerError(HandlerControllerResponse.InvalidMethodParameters, package.Route);
    }

    private static object? DeserializeToParameter(string json, Type paramType)
    {
        try
        {
            var param = JsonSerializer.Deserialize(json, paramType);
            return param;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static (string? json, Route route) GetJsonResponse(Connection connection, object parameter, object inst, MethodInfo mi)
    {
        try
        {
            var result = mi.Invoke(inst, new object?[] { connection, parameter }) as Response;
            if(result is null)
                return (string.Empty, Route.Default);
            var json = JsonSerializer.Serialize(result.Value);
            return (json, result.Route);
        }
        catch (Exception)
        {
            return (null, Route.Default);
        }
    }

    public static HandlersController operator +(HandlersController rhc, Type handler)
    {
        rhc.AddHandler(handler);
        return rhc;
    }

    public static HandlersController operator -(HandlersController rhc, Type handler)
    {
        rhc.RemoveHandler(handler);
        return rhc;
    }

    public static HandlersController operator +(HandlersController rhc, object handler)
    {
        rhc.AddHandler(handler.GetType());
        return rhc;
    }

    public static HandlersController operator -(HandlersController rhc, object handler)
    {
        rhc.RemoveHandler(handler.GetType());
        return rhc;
    }

    public void Dispose()
    {
        ThrownException = null;
        _activeHandlers.Clear();
    }
}
