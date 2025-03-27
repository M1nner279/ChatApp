using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Models;

namespace ChatApp.Services;

public class ConnectionManager
{
    public List<ChatNode> Nodes { get; private set; } = new List<ChatNode>();

    private readonly UdpService _udpService;
    private readonly TcpService _tcpService;
    private readonly string _myName; // Имя текущего узла

    public ConnectionManager(UdpService udpService, TcpService tcpService, string myName)
    {
        _udpService = udpService;
        _tcpService = tcpService;
        _myName = myName;

        // Подписка на событие обнаружения нового узла
        _udpService.OnNodeDiscovered += OnNodeDiscovered;

        // Подписка на события TCP
        _tcpService.OnNodeConnected += OnNodeConnected;
        _tcpService.OnNodeDisconnected += OnNodeDisconnected;
    }

    private void OnNodeDiscovered(ChatNode node)
    {
        if (!Nodes.Contains(node))
        {
            Nodes.Add(node);
            Task.Run(async () =>
            {
                await _tcpService.ConnectToNodeAsync(node, _myName);
            } );
        }
    }

    private void OnNodeConnected(ChatNode node)
    {
        var existing = Nodes.Find(n => n.IpAddress == node.IpAddress);
        if (existing != null)
        {
            existing.IsConnected = true;
            existing.UserName = node.UserName; // обновляем состояние узла
        }
        else
        {
            Nodes.Add(node); // добавляем в список
        }
    }

    private void OnNodeDisconnected(ChatNode node)
    {
        var existing = Nodes.Find(n => n.IpAddress == node.IpAddress);
        if (existing != null)
        {
            existing.IsConnected = false;
        }
    }
}