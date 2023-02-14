using ASiNet.Connector.Enums;
using System.Net.Sockets;
using System.Text.Json;

namespace ASiNet.Connector;
public delegate void AcceptedNewResponsePackage(Package pack);
public partial class Connection : IDisposable
{
    /// <summary>
    /// Создать неподключенный клиент.
    /// </summary>
    public Connection()
    {
        HandlersController = new();
        Status = ConnectionStatus.Disconnected;
        _tcpClient = new();
        InitReader();
        InitWriter();
        InitTimer();
        CheckForUpdatesEvery = 100;
    }

    /// <summary>
    /// Создать неподключенный клиент со статусом ошибки подключения.
    /// </summary>
    /// <param name="status"></param>
    internal Connection(ConnectionStatus status)
    {
        HandlersController = new();
        Status = status;
        _tcpClient = new();
        InitReader();
        InitWriter();
        InitTimer();
        CheckForUpdatesEvery = 100;
    }
    /// <summary>
    /// Создать клиент на основе активного <see cref="TcpClient"/>.
    /// </summary>
    /// <param name="client">Подключённый <see cref="TcpClient"/></param>
    internal Connection(TcpClient client)
    {
        HandlersController = new();
        _tcpClient = client;
        if (client.Connected)
        {
            Status = ConnectionStatus.Connected;
            _stream = _tcpClient.GetStream();
        }
        else
            Status = ConnectionStatus.Disconnected;
        InitReader();
        InitWriter();
        InitTimer();
        CheckForUpdatesEvery = 100;
    }
    /// <summary>
    /// Интервал времени через который клиент снова попытается прочитать <see cref="TcpClient"/>
    /// </summary>
    public int CheckForUpdatesEvery
    {
        get => _checkForUpdatesEvery;
        set
        {
            _checkForUpdatesEvery = value;
            _timer.Value.Change(0, value);
        }
    }
    /// <summary>
    /// Событие на которое следует подписаться для получения ответов от удаллёного клиента.
    /// </summary>
    public HandlersController HandlersController { get; set; }
    /// <summary>
    /// Текущий статус клиента.
    /// </summary>
    public ConnectionStatus Status { get; private set; }
    /// <summary>
    /// Время на чтение.
    /// </summary>
    public int ReadTimeout
    {
        get => _stream?.ReadTimeout ?? -1;
        set => _stream.ReadTimeout = value;
    }
    /// <summary>
    /// Время на запись.
    /// </summary>
    public int WriteTimeout
    {
        get => _stream?.WriteTimeout ?? -1;
        set => _stream.WriteTimeout = value;

    }

    private int _checkForUpdatesEvery;

    private TcpClient _tcpClient;
    private NetworkStream _stream = null!;
    private Lazy<BinaryReader> _reader = null!;
    private Lazy<BinaryWriter> _writer = null!;
    private Lazy<Timer> _timer = null!;

    private readonly object _writeLocker = new();
    private readonly object _readLocker = new();
    /// <summary>
    /// Отправить запрос на удалённый клиент.
    /// </summary>
    /// <param name="obj">Объект который следует отправить.</param>
    /// <param name="router">Имя роутера на который требуется отправить запрос.</param>
    /// <param name="method">Имя метода на который следует отправить запрос.</param>
    public void Send<Tobj>(Tobj obj, Route route)
    {
        if (Status != ConnectionStatus.Connected)
            return;

        try
        {
            lock (_writeLocker)
            {
                var objJson = JsonSerializer.Serialize(obj);
                var package = Package.CreateRequest(objJson, route);
                var json = JsonSerializer.Serialize(package);
                _writer.Value.Write(json);
            }
        }
        catch (ObjectDisposedException)
        {
            Status = ConnectionStatus.Disconnected;
            Dispose();
        }
        catch (IOException ex) when (ex.InnerException is SocketException)
        {
            Status = ConnectionStatus.Disconnected;
            Dispose();
        }
        catch (IOException ex) when (ex.InnerException is not SocketException)
        { }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Отправить ответ на удалённый клиент.
    /// </summary>
    /// <param name="package">Пакет.</param>
    internal void Send(Package package)
    {
        if (Status != ConnectionStatus.Connected)
            return;

        try
        {
            lock (_writeLocker)
            {
                var json = JsonSerializer.Serialize(package);
                _writer.Value.Write(json);
            }
        }
        catch (ObjectDisposedException)
        {
            Status = ConnectionStatus.Disconnected;
            Dispose();
        }
        catch (IOException ex) when (ex.InnerException is SocketException)
        {
            Status = ConnectionStatus.Disconnected;
            Dispose();
        }
        catch (IOException ex) when (ex.InnerException is not SocketException)
        { }
        catch
        {
            throw;
        }
    }
    /// <summary>
    /// Проверяет наличие обновлений.
    /// </summary>
    private void OnUpdates(object? state)
    {
        if (Status != ConnectionStatus.Connected || !_stream.DataAvailable)
            return;

        try
        {
            lock (_readLocker)
            {
                while (_stream.DataAvailable)
                {
                    var json = _reader.Value.ReadString();
                    var package = JsonSerializer.Deserialize<Package>(json);
                    if(package is null)
                        continue;
                    if (package.Type == PackageType.Response)
                    {
                        HandlersController.ExecuteHandler(this, package);
                        return;
                    }

                    if (package.Type == PackageType.Request)
                    {
                        var result = RouterController.DirectToRouter(this, package);
                        Send(result);
                        return;
                    }
                }
            }
        }
        catch (ObjectDisposedException)
        {
            Status = ConnectionStatus.Disconnected;
            Dispose();
        }
        catch (IOException ex) when (ex.InnerException is SocketException)
        {
            Status = ConnectionStatus.Disconnected;
            Dispose();
        }
        catch (IOException ex) when (ex.InnerException is not SocketException)
        { }
        catch
        {
            throw;
        }
    }

    private Lazy<Timer> InitTimer() => _timer = new(() => new(OnUpdates, null, 0, _checkForUpdatesEvery));
    private Lazy<BinaryReader> InitReader() => _reader = new(() => new BinaryReader(_stream));
    private Lazy<BinaryWriter> InitWriter() => _writer = new(() => new BinaryWriter(_stream));
    /// <summary>
    /// Прервать подключение и удалить все ресурсы. В том числе и из активного <see cref="Connector.HandlersController"/>
    /// </summary>
    public void Dispose()
    {
        if (_reader.IsValueCreated)
            _reader.Value.Dispose();
        if (_writer.IsValueCreated)
            _writer.Value.Dispose();
        if (_timer.IsValueCreated)
            _timer.Value.Dispose();
        HandlersController.Dispose();
        _stream.Dispose();
        _tcpClient.Dispose();
    }
}
