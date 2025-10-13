namespace FlexNet.Domain.Entities;

public class UserDescription
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int Age { get; set; }
    public string? Gender { get; set; }
    public string Education { get; set; }
    public string Purpose { get; set; }
    public User? User { get; set; }
}