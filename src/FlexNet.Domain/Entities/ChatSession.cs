namespace FlexNet.Domain.Entities;

public class ChatSession
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Summary { get; set; }
    public DateTime StartedTime { get; set; }  
    public DateTime? EndedTime { get; set; }
    public User? User { get; set; }
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}