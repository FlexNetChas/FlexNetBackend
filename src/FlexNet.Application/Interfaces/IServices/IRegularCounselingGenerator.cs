using FlexNet.Application.DTOs.AI;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;

namespace FlexNet.Application.Interfaces.IServices;

public interface IRegularCounselingGenerator
{
  Task<Result<string>> GenerateAsync(string userMsg, IEnumerable<ConversationMessage> conversationHistory, UserContextDto userContextDto);
  IAsyncEnumerable<Result<string>> GenerateStreamingAsync(string userMsg, IEnumerable<ConversationMessage> conversationHistory, UserContextDto userContextDto);
}