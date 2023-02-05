using ASiNet.Connector;
using ASiNet.Connector.Attributes;

var connection = await Connection.WaitConnect(50500);

Console.WriteLine($"{connection.Status}");

Console.Read();

[Router("test1")]
class TestRouter
{
    [RouterMethod("test")]
    public string TestMethod(Connection connection, string text)
    {
        Console.WriteLine(text);
        return "Done!";
    }
}