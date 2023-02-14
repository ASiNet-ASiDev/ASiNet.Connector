using ASiNet.Connector;
using System;

var connection = await Connection.Connect("localhost", 50500);

Console.WriteLine(connection.Status);
if (connection.Status != ASiNet.Connector.Enums.ConnectionStatus.Connected)
    Console.Read();


connection.WriteTimeout = 1000;
connection.ReadTimeout = 1000;
connection.HandlersController += (new("test1", "test"), OnPackage);

connection.Send("Hello World!", new("test1", "test"));
Console.Read();

void OnPackage(Connection connection, Package pack, string str)
{
    Console.WriteLine($"[{pack.Route}] [{pack.RouterControllerResult}] [{pack.Json}]");
}

Console.WriteLine($"{connection.Status}");

Console.ReadKey();