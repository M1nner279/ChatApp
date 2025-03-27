using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Models;

namespace ChatApp.Services;

public class TcpService
{
    public event Action<ChatMessage> OnMessageReceived;
    public event Action<ChatNode> OnNodeConnected;
    public event Action<ChatNode> OnNodeDisconnected;

    private TcpListener _listener;
    private CancellationTokenSource _cts;
    private readonly int _port = 11001;
    
    private ConcurrentDictionary<string, TcpClient> _clients = new ConcurrentDictionary<string, TcpClient>();

    public TcpService()
    {
        _listener = new TcpListener(IPAddress.Any, _port);
    }

    public void startServer()
    {
        _cts = new CancellationTokenSource();
        _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        try
        {
            _listener.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error TcpStart: {ex.Message}");
        }
        Task.Run(() => AcceptClientAsync(_cts.Token));
    }
    
    private async Task AcceptClientAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                Task.Run(() => HandleClientAsync(client, token));
            }
        }
        catch (ObjectDisposedException) {}
        catch (Exception ex)
        {
            Console.WriteLine($"Tcp Accept Error: {ex.Message}");       
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        string clientIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
        NetworkStream stream = null;
        string nodeName = "";
        try
        {
            stream = client.GetStream();
            nodeName = await ReadNodeNameAsync(stream, token);

            ChatNode newNode = new ChatNode
            {
                IpAddress = clientIp,
                IsConnected = true,
                UserName = nodeName
            };

            _clients.TryAdd(clientIp, client);
            OnNodeConnected?.Invoke(newNode);
            await ProcessMessagesAsync(stream, clientIp, nodeName, token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TCP client error ({clientIp}): {ex.Message}");
        }
        finally
        {
            _clients.TryRemove(clientIp, out _);
            OnNodeDisconnected?.Invoke(new ChatNode { IpAddress = clientIp, UserName = nodeName ?? "", IsConnected = false });
            client.Close();
        }
    }
    private async Task<string> ReadNodeNameAsync(NetworkStream client, CancellationToken token)
    {
        try
        {
            byte[] lengthBuffer = new byte[4];
            await client.ReadAsync(lengthBuffer, 0, 4, token);
            int nameLength = BitConverter.ToInt32(lengthBuffer, 0);

            byte[] nameBuffer = new byte[nameLength];
            await client.ReadAsync(nameBuffer, 0, nameLength, token);
            return Encoding.UTF8.GetString(nameBuffer);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error reading name: {ex.Message}", ex);
        }
    }
    private async Task ProcessMessagesAsync(NetworkStream stream, string clientIp, string nodeName,
        CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                //обработка сообщения
                byte[] msgLengthBuffer = new byte[4];
                int bytesRead = await stream.ReadAsync(msgLengthBuffer, 0, 4, token);
                if (bytesRead == 0) break; // соединение было прервано

                int msgLength = BitConverter.ToInt32(msgLengthBuffer, 0);
                byte[] msgBuffer = new byte[msgLength];
                bytesRead = await stream.ReadAsync(msgBuffer, 0, msgLength, token);
                if (bytesRead == 0) break;

                string messageContent = Encoding.UTF8.GetString(msgBuffer);
                ChatMessage chatMessage = new ChatMessage
                {
                    Content = messageContent,
                    SenderIp = clientIp,
                    SenderName = nodeName,
                    TimeStamp = DateTime.Now,
                    Type = MessageType.Text
                };
                OnMessageReceived?.Invoke(chatMessage);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Message processing error ({clientIp}): {ex.Message}");
            throw;
        }
    }
    
    public async Task ConnectToNodeAsync(ChatNode node, string myName)
    {
        try
        {
            TcpClient client = new TcpClient();
            await client.ConnectAsync(IPAddress.Parse(node.IpAddress), _port);
            NetworkStream stream = client.GetStream();

            byte[] nameBytes = Encoding.UTF8.GetBytes(myName);
            byte[] lengthBytes = BitConverter.GetBytes(nameBytes.Length);
            await stream.WriteAsync(nameBytes);
            await stream.WriteAsync(lengthBytes);
            
            //save connection on list
            _clients.TryAdd(node.IpAddress, client);
            node.IsConnected = true;
            OnNodeConnected?.Invoke(node);
            
            //запуск прослушки на этот клиент
            _ = Task.Run(() => HandleClientAsync(client, CancellationToken.None));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error Tcp connect to node: {ex.Message}");
        }
    }

    public async Task SendMessageAsync(ChatMessage message)
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(message.Content);
        byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);
        byte[] data = new byte[4 + messageBytes.Length];
        Array.Copy(lengthBytes, data, 4);
        Array.Copy(messageBytes, 0, data, 4, messageBytes.Length);

        foreach (var kvp in _clients)
        {
            try
            {
                NetworkStream stream = kvp.Value.GetStream();
                await stream.WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to {kvp.Key}: {ex.Message}");
            }
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
        _listener.Stop();
        foreach (var client in _clients.Values)
            client.Close();
    }
}