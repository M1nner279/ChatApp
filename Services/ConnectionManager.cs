using System.Collections.Generic;
using ChatApp.Models;


namespace ChatApp.Services;

public class ConnectionManager
{
    public List<ChatNode> Nodes { get; private set; } = new List<ChatNode>();

    private readonly string userName;
    private readonly UdpService udpService;


    public ConnectionManager(string userName, UdpService udpService)
    {
        this.userName = userName;
        this.udpService = udpService;

        udpService.OnNodeDiscovered += OnNodeDiscovered;
    }


    private void OnNodeDiscovered(ChatNode node)
    {
        if (!Nodes.Exists(n => n.IpAddress == node.IpAddress))
        {
            Nodes.Add(node);
            // запуск Tcp соединения
            
        }
    }
}