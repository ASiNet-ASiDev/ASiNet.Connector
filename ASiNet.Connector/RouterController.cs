using ASiNet.Connector.Attributes;
using ASiNet.Connector.Enums;
using System.Reflection;
using System.Text.Json;

namespace ASiNet.Connector;
public static class RouterController
{
    public static Package DirectToRouter(Connection connection, Package pack)
    {
        var router = FindRouter(pack.RouterName);
        if (router is null)
            return Package.ErrorRCResponse(RouterControllerResponse.RouterNotFound, pack.RouterName, pack.MethodName);

        var routerResult = CreateRouterAndGetMethod(router, pack.MethodName);
        if (routerResult.instance is null)
            return Package.ErrorRCResponse(RouterControllerResponse.FailedToCreateRouterInstance, pack.RouterName, pack.MethodName);
        if (routerResult.method is null)
            return Package.ErrorRCResponse(RouterControllerResponse.MethodNotFound, pack.RouterName, pack.MethodName);

        var parameters = routerResult.method.GetParameters();
        if (parameters.Length == 2 && parameters[0].ParameterType == typeof(Connection))
        {
            var param = DeserializeToParameter(pack.Json, parameters[1].ParameterType);
            if (param is null)
                return Package.ErrorRCResponse(RouterControllerResponse.DeserializeJsonRequestError, pack.RouterName, pack.MethodName);
            var json = GetJsonResponse(connection, param, routerResult.instance, routerResult.method);

            if (json is null)
                return Package.ErrorRCResponse(RouterControllerResponse.SerializeResponseToJsonError, pack.RouterName, pack.MethodName);

            var package = new Package()
            {
                Type = PackageType.Response,
                RouterControllerResult = RouterControllerResponse.Done,
                Json = json,
                MethodName = pack.MethodName,
                RouterName = pack.RouterName,
            };
            return package;
        }
        else
            return Package.ErrorRCResponse(RouterControllerResponse.InvalidMethodParameters, pack.RouterName, pack.MethodName);
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

    private static string? GetJsonResponse(Connection connection, object parameter, object inst, MethodInfo mi)
    {
        try
        {
            var result = mi.Invoke(inst, new object?[] { connection, parameter });
            var json = JsonSerializer.Serialize(result);
            return json;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static Type? FindRouter(string routerName)
    {
        var assembly = Assembly.GetEntryAssembly();
        var router = assembly?.GetTypes().FirstOrDefault(x => (x.GetCustomAttribute<RouterAttribute>() is RouterAttribute attr) && attr.RouterName == routerName);
        return router;
    }

    private static (MethodInfo? method, object? instance) CreateRouterAndGetMethod(Type routerType, string methodName)
    {
        object? inst = null;
        try
        {
            inst = Activator.CreateInstance(routerType);
            var method = routerType.GetMethods().FirstOrDefault(x => (x.GetCustomAttribute<RouterMethodAttribute>() is RouterMethodAttribute attr) && attr.Name == methodName);
            return (method, inst);
        }
        catch
        {
            return (null, inst);
        }
    }

}
