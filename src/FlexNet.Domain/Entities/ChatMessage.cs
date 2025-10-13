namespace FlexNet.Domain.Entities;

public class ChatMessage
{
    public int Id { get; set; }
    public string MessageText { get; set; }
    public DateTime TimeStamp { get; set; }
    public DateTime? LastUpdated { get; set; }
    public int ChatSessionId { get; set; }
    public ChatSession? ChatSession { get; set; }
}