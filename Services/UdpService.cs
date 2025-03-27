using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Models;

namespace ChatApp.Services;

public class UdpService
{
    public event Action<ChatNode> OnNodeDiscovered;

    private UdpClient _udpClient;
    private CancellationTokenSource _cts;
    private readonly int _port = 11000;

    public UdpService()
    {
        _udpClient = new UdpClient();
        _udpClient.EnableBroadcast = true;
        _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        try
        {
            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, _port));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Bind: {ex.Message}");
        }
        
    }

    public void StartListening()
    {
        _cts = new CancellationTokenSource();
        Task.Run(() => ListenAsync(_cts.Token));
    }
    
    private async Task ListenAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                UdpReceiveResult result = await _udpClient.ReceiveAsync();
                string userName = Encoding.UTF8.GetString(result.Buffer);
                Console.WriteLine(userName);
                ChatNode discoveredNode = new ChatNode();
                OnNodeDiscovered?.Invoke(discoveredNode);
            }
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Udp BroadCast Error: {ex.Message}");
        }
        
    }

    public async Task BroadcastPresenceAsync(string userName)
    {
        byte[] data = Encoding.UTF8.GetBytes(userName);
        IPEndPoint broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, _port);
        try
        {
            await _udpClient.SendAsync(data, data.Length, broadcastEndpoint);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UDP Broadcast error: {ex.Message}");
        }
    }

    // stop listening
    public void Stop()
    {
        _cts.Cancel();
        _udpClient.Client.Close();
    }

}