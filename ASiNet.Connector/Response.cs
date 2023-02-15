namespace ASiNet.Connector;
public class Response
{
    public Response(object value, Route route)
    {
        Value = value;
        Route = route;
    }

    public Response(object value, string methodName)
    {
        Value = value;
        Route = new(string.Empty, methodName, -1);
    }

    public Route Route { get; set; }

    public object Value { get; set; }
}
