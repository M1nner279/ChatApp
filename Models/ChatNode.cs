namespace ChatApp.Models;

public class ChatNode
{
    public string IpAddress { get; set; }
    public string UserName { get; set; }
    public bool IsConnected { get; set; }

    public override string ToString() => $"{UserName}, ({IpAddress})";
}