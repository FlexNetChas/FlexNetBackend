namespace FlexNet.Domain.Entities;

public class Avatar
{
   public int Id { get; set; }
   public int UserId { get; set; }
   public string? Style { get; set; }
   public string? Personality { get; set; }
   public bool VoiceEnabled { get; set; }     
   public string? VoiceSelection { get; set; }

   public User? User { get; set; }
}