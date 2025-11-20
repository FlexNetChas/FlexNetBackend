using System.Text;
using FlexNet.Application.DTOs.AI;
using FlexNet.Application.Models;

namespace FlexNet.Application.Services.Prompts;

public static class SystemPrompts
{
    public static string BuildSystemPrompt(UserContextDto userContext)
    {
        var prompt = new StringBuilder();
        
        prompt.AppendLine("Du är FlexNet - en erfaren studie- och yrkesvägledare för svenska gymnasieelever.");
        prompt.AppendLine();
        prompt.AppendLine("Din roll:");
        prompt.AppendLine("- Du hjälper elever hitta rätt utbildning baserat på deras intressen");
        prompt.AppendLine("- Du ger konkret, användbar vägledning med nästa steg");
        prompt.AppendLine("- Du är varm, stödjande och uppmuntrande");
        prompt.AppendLine();
        prompt.AppendLine("Riktlinjer:");
        prompt.AppendLine("- Svara alltid på svenska");
        prompt.AppendLine("- Håll svar kortfattade (3-5 meningar) om inte mer information behövs");
        prompt.AppendLine("- Ge konkreta råd snarare än generella svar");
        prompt.AppendLine();
        prompt.AppendLine($"Kontext: Du hjälper just nu en {userContext.Age}-årig elev.");
        
        return prompt.ToString();
    }
    public static void AppendConversationHistory(
        StringBuilder prompt,
        IEnumerable<ConversationMessage> history,
        int maxMessages = 3)
    {
        if (!history.Any()) return;
        
        prompt.AppendLine("Tidigare i konversationen:");
        foreach (var msg in history.TakeLast(maxMessages))
        {
            var speaker = msg.Role == "user" ? "Elev" : "FlexNet";
            prompt.AppendLine($"{speaker}: {msg.Content}");
        }
        prompt.AppendLine();
    }
}