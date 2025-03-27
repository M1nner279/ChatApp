using System.Collections.ObjectModel;
using ChatApp.Models;

namespace ChatApp.Services;

public class HistoryService
{
    public ObservableCollection<ChatMessage> ChatHistory { get; private set; } =
        new ObservableCollection<ChatMessage>();

    public void AddMessage(ChatMessage message)
    {
        ChatHistory.Add(message);
    }
    
    //сериализация и десериализация в json
}