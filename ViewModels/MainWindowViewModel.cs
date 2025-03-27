using System.Collections.ObjectModel;
using ChatApp.Models;
using ChatApp.Services;
using ReactiveUI;

namespace ChatApp.ViewModels;

public partial class MainWindowViewModel : ReactiveObject
{
    private readonly UdpService _udpService;

    private readonly string userName = "Host";
    
    public string Greeting { get; } = "Welcome to Chat";
    public ObservableCollection<ChatMessage> ChatMessages;
    

    public MainWindowViewModel()
    {
        _udpService = new UdpService();
        
        _udpService.StartListening();
        
        _ = _udpService.BroadcastPresenceAsync(userName);
    }
}