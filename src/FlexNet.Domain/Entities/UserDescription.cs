namespace FlexNet.Domain.Entities;

public class UserDescription
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int Age { get; set; }
    public string? Gender { get; set; }
    public required string Education { get; set; }
    public required string Purpose { get; set; }
    public User? User { get; set; }
}