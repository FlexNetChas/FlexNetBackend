namespace FlexNet.Application.Models;

public class SendMessageRequest
{
        public string Message { get; set; } = string.Empty;
        public int? Age { get; set; } // Optional for now
}