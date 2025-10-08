namespace FlexNet.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    // Navigation properties put back
    public ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();
    public Avatar? Avatar { get; set; }
    public UserDescription? UserDescription { get; set; }
}