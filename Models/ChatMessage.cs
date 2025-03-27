using System;

namespace ChatApp.Models;

public class ChatMessage
{
    public MessageType Type { get; set; }
    public string SenderName { get; set; }
    public string SenderIp { get; set; }
    public DateTime TimeStamp { get; set; }
    public string Content { get; set; }

    public ChatMessage(MessageType type, string senderIp, string senderName, DateTime timeStamp, string content)
    {
        Type = type;
        SenderName = senderName;
        SenderIp = senderIp;
        TimeStamp = timeStamp;
        Content = content;
    }
    
    
}