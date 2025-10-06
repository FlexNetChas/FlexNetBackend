namespace FlexNet.Domain.Entities;

public class Avatar
{
   public int Id { get; set; }
   public int UserId { get; set; }
   public string Style { get; set; }
   public string Personality { get; set; }
   public string Voice { get; set; }

   public User? User { get; set; }
}