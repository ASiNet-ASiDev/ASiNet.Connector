using ASiNet.Connector.Enums;
using System.Net;
using System.Net.Sockets;

namespace ASiNet.Connector;
/// <summary>
/// Класс подключения, используйте статические методы что бы создать подключение.
/// </summary>
public partial class Connection : IDisposable
{
    /// <summary>
    /// Создать подключение.
    /// </summary>
    /// <param name="host">Аддрес.</param>
    /// <param name="port">Порт.</param>
    /// <param name="timeout">Время ожидания подключения Default: 5 seconds</param>
    /// <param name="token">Токен для отмены подключения.</param>
    /// <returns>Подключение.</returns>
    public static async Task<Connection> Connect(string host, int port, int timeout = 5000, CancellationToken token = default)
    {
        try
        {
            var cancellationCompletionSource = new TaskCompletionSource<bool>();

            var client = new TcpClient();
            var task = client.ConnectAsync(host, port);

            using (var cts = new CancellationTokenSource(timeout))
            using (token.Register(() => cancellationCompletionSource.TrySetResult(false)))
            using (cts.Token.Register(() => cancellationCompletionSource.TrySetResult(true)))
                if (task != await Task.WhenAny(task, cancellationCompletionSource.Task))
                {
                    client.Dispose();
                    throw cancellationCompletionSource.Task.Result ? new TimeoutException() : new OperationCanceledException(token);
                }
            if (!client.Connected)
                throw new SocketException();
            var connection = new Connection(client);
            return connection;
        }
        catch (OperationCanceledException)
        {
            return new(ConnectionStatus.ConnectionCanceled);
        }
        catch (TimeoutException)
        {
            return new(ConnectionStatus.ConnectionTimeout);
        }
        catch (SocketException)
        {
            return new(ConnectionStatus.ConnectionError);
        }
        catch
        {
            throw;
        }
    }
    /// <summary>
    /// Отключиться (на данный момент не реализован и просто вызывает метод Dispose)
    /// </summary>
    /// <param name="connection">Активное подключение которое надо прервать.</param>
    /// <returns></returns>
    public static async Task Disconnect(Connection connection)
    {
        await Task.Run(() =>
        {
            connection.Dispose();
        });
    }
    /// <summary>
    /// Ожидать подключение.
    /// </summary>
    /// <param name="port">Порт по которому будет производиться ожидание подключения.</param>
    /// <param name="token">Токен для отмены ожидания подключения.</param>
    /// <returns></returns>
    public static async Task<Connection> WaitConnect(int port, CancellationToken token = default)
    {
        try
        {
            var tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            var client = await tcpListener.AcceptTcpClientAsync(token);
            tcpListener.Stop();
            if (client?.Connected ?? false)
                return new(client);
            else
                throw token.IsCancellationRequested ? new OperationCanceledException(token) : new SocketException();
        }
        catch (OperationCanceledException)
        {
            return new(ConnectionStatus.ConnectionCanceled);
        }
        catch (SocketException)
        {
            return new(ConnectionStatus.ConnectionError);
        }
        catch
        {
            throw;
        }
    }
}