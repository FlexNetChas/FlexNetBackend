namespace FlexNet.Domain.Entities;

public class User
{
    public int Id{ get; set; }
    public string FirstName{ get; set; }
    public string LastName{ get; set; }
    public string Email{ get; set; }
    public string Role { get; set; } = "User";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string PasswordHash { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();
    public Avatar? Avatar { get; set; }
    public UserDescription? UserDescription { get; set; }


}