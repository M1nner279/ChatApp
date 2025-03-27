using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using ChatApp.Models;
using ChatApp.Services;
using ReactiveUI;

namespace ChatApp.ViewModels;

public partial class MainWindowViewModel : ReactiveObject
{
    private readonly UdpService _udpService;
    private readonly TcpService _tcpService;
    private readonly HistoryService _history;
    private readonly ConnectionManager _connectionManager;

    public string userName = "User";
    
    public string Greeting { get; } = "Welcome to Chat";
    public ObservableCollection<ChatMessage> ChatMessages => _history.ChatHistory;
    

    public MainWindowViewModel()
    {
        _udpService = new UdpService();
        
        _udpService.StartListening();
        
        _ = _udpService.BroadcastPresenceAsync("Input");
    }
}