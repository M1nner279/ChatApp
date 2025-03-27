using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Reactive;
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

    public string userName = "User1";
    
    public string Greeting { get; } = "Welcome to Chat";
    public ObservableCollection<ChatMessage> ChatMessages => _history.ChatHistory;
    
    private string _messageToSend;
    public string MessageToSend
    {
        get => _messageToSend;
        set => this.RaiseAndSetIfChanged(ref _messageToSend, value);
    }   
    public ReactiveCommand<Unit, Unit> SendMessageCommand { get; }
    public MainWindowViewModel()
    {
        _history = new HistoryService();
        _tcpService = new TcpService();
        _udpService = new UdpService();
        _connectionManager = new ConnectionManager(_udpService, _tcpService, userName);

        _tcpService.OnMessageReceived += message =>
        {
            _history.AddMessage(message);
        };
        
        SendMessageCommand = ReactiveCommand.Create(SendMessage);
        
        _udpService.StartListening();
        _tcpService.startServer();
        
        _ = _udpService.BroadcastPresenceAsync(userName);
    }
    
    private void SendMessage()
    {
        if (string.IsNullOrWhiteSpace(MessageToSend))
            return;

        var message = new ChatMessage
        {
            Type = MessageType.Text,
            SenderName = userName,
            SenderIp = "наш IP", // Здесь можно реализовать получение реального IP-адреса
            TimeStamp = DateTime.Now,
            Content = MessageToSend
        };

        // Добавление сообщения в локальную историю
        _history.AddMessage(message);

        // Отправка сообщения через TCP
        _ = _tcpService.SendMessageAsync(message);

        // Очистка поля ввода
        MessageToSend = string.Empty;
    }
}