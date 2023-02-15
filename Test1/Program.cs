using ASiNet.Connector;
using ASiNet.Connector.Attributes;
using System;

var connection = await Connection.Connect("localhost", 50500);

Console.WriteLine(connection.Status);
if (connection.Status != ASiNet.Connector.Enums.ConnectionStatus.Connected)
    Console.Read();


connection.WriteTimeout = 1000;
connection.ReadTimeout = 1000;

connection.HandlersController += new TestHandler();

connection.SendRequest("Hello World!", new("test1", "test", -1));
Console.Read();

[Handler("test1")]
class TestHandler : IDisposable
{
    [HandlerMethod("response")]
    public void TestMethodResponse(Connection connection, string text)
    {
        Console.WriteLine($"Response: {text}");
        connection.HandlersController.CloseHandler(connection, this);
    }

    public void Dispose()
    {
        Console.WriteLine("Handler Closed!");
    }
}