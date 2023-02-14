using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.Connector;
public class ConnectionListener
{
    public ConnectionListener(int port, string ip = "0.0.0.0", int updatePeriod = 60000)
    {
        _listener = new(IPAddress.Parse(ip), port);
        _connections = new();
        _updater = new(OnUpdate, null, 0, updatePeriod);
    }

    public bool WaitConnections
    {
        get => _waitConnections;
        set
        {
            if (_waitConnections != value)
            {
                if (_waitConnections)
                {
                    _listener.Start();
                    WaitConnection();
                }
                else
                {
                    _listener.Stop();
                }
            }
            _waitConnections = value;
        }
    }

    private Timer _updater;
    private List<Connection> _connections;
    private bool _waitConnections;

    private TcpListener _listener;

    private readonly object _clientsListLocker = new object();

    private async void WaitConnection()
    {
        while (_waitConnections)
        {
            var tcp = await _listener.AcceptTcpClientAsync();
            _connections.Add(new(tcp));
        }
    }

    private void OnUpdate(object? obj)
    {
        UpdateClientsList();
    }

    private void UpdateClientsList()
    {
        lock (_clientsListLocker)
        {
            var time = DateTime.UtcNow;
            var clients = _connections.Where(x => x.Status != Enums.ConnectionStatus.Connected).ToList();
            foreach (var item in clients)
            {
                item.Dispose();
                _connections.Remove(item);
            }
        }
    }
}
