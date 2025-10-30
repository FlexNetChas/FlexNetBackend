using System.Security;
using System.Text;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;

namespace FlexNet.Application.Services;

public class AiContextBuilder
{
    public string BuildContext(
        StudentContext studentContext,
        IEnumerable<ConversationMessage> conversationHistory,
        string currentMessage)
    {
        var sb = new StringBuilder();
        AppendStudentContext(sb, studentContext);
        sb.AppendLine();

        if (conversationHistory.Any())
        {
            AppendConversationHistory(sb, conversationHistory);
            sb.AppendLine();
        }
        AppendRoleDefinition(sb);
        sb.AppendLine();
        
        AppendCurrentMessage(sb, currentMessage);
        
        return sb.ToString();
    }

    private void AppendStudentContext(StringBuilder sb, StudentContext context)
    {
        sb.AppendLine("<student_context>");
        sb.AppendLine($"    <age>{context.Age}</age>");

        if (!string.IsNullOrEmpty(context.Education))
            sb.AppendLine($"    <education>{EscapeXml(context.Education)}</education>)");
        if(!string.IsNullOrEmpty(context.Purpose))
            sb.AppendLine($"    <purpose>{EscapeXml(context.Purpose)}</purpose>");
        if(!string.IsNullOrEmpty(context.Gender))
            sb.AppendLine($"    <gender>{EscapeXml(context.Gender)}</gender>");
        
        sb.AppendLine("</student_context>");
    }

    private void AppendConversationHistory(StringBuilder sb, IEnumerable<ConversationMessage> history)
    {
        sb.AppendLine("<conversation_history>");

        var recentMessages = history.TakeLast(5);

        foreach (var message in recentMessages)
        {
            sb.AppendLine($"  <message role=\"{message.Role}\">");
            sb.AppendLine($"    {EscapeXml(message.Content)}");
            sb.AppendLine($"  </message>");
        }
        sb.AppendLine("</conversation_history>");
    }

    private void AppendRoleDefinition(StringBuilder sb)
    {
        sb.AppendLine("<role>");
        sb.AppendLine("You are a supportive and empathetic school counsellor specializing in academic and career guidance for students in Sweden.");
        sb.AppendLine();
        sb.AppendLine("Your approach:");
        sb.AppendLine("- Listen actively and validate student concerns");
        sb.AppendLine("- Ask clarifying questions to understand their situation");
        sb.AppendLine("- Provide practical, actionable advice");
        sb.AppendLine("- Encourage self-reflection and critical thinking");
        sb.AppendLine("- Use age-appropriate language and examples");
        sb.AppendLine();
        sb.AppendLine("Your expertise includes:");
        sb.AppendLine("- Study techniques and time management");
        sb.AppendLine("- Career exploration and university selection");
        sb.AppendLine("- Academic goal setting and motivation");
        sb.AppendLine("- Stress management related to academics");
        sb.AppendLine();
        sb.AppendLine("Important boundaries:");
        sb.AppendLine("- Focus on academic and career topics");
        sb.AppendLine("- If a student mentions self-harm, severe anxiety, or crisis situations, acknowledge their feelings and strongly encourage them to speak with a trusted adult, school counselor, or professional immediately");
        sb.AppendLine("- Do not provide medical, legal, or financial advice");
        sb.AppendLine("- Encourage students to verify important decisions with parents/guardians");
        sb.AppendLine();
        sb.AppendLine("Always maintain a supportive, non-judgmental tone and respect student autonomy.");
        sb.AppendLine("</role>");
    }

    private void AppendCurrentMessage(StringBuilder sb, string message)
    {
        sb.AppendLine("<current_message>");
        sb.AppendLine(EscapeXml(message));
        sb.AppendLine("</current_message>");
    }

    private static string EscapeXml(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        
        return SecurityElement.Escape(value);
    }

}