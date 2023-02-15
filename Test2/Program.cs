using ASiNet.Connector;
using ASiNet.Connector.Attributes;
using ASiNet.Connector.Enums;

var connection = await Connection.WaitConnect(50500);

Console.WriteLine($"{connection.Status}");

connection.HandlersController +=  new TestHandler();

Console.Read();

[Handler("test1")]
class TestHandler : IDisposable
{
    [HandlerMethod("test")]
    public Response TestMethod(Connection connection, string text)
    {
        Console.WriteLine($"Request: {text}");
        return new("Done Response!", "response");
    }

    public void Dispose()
    {
        Console.WriteLine("Handler Closed!");
    }
}