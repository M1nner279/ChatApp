using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ChatApp.Models;

public class ChatClient
{
    public event EventHandler<Message> MessageReceived;
    
    private readonly string _localIp;
    private readonly string _localPort;

    public ChatClient(string localIp, string localPort)
    {
        _localPort = localPort;
        _localIp = localIp;
    }

    public async Task SendMessageAsync()
    {
        // sending in tcp
        using TcpClient client = new TcpClient();
    }
}