using FlexNet.Application.DTOs.AI;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;

namespace FlexNet.Infrastructure.Interfaces;

public interface IGuidanceRouter
{
  Task<Result<string>> RouteAndExecuteAsync(string userMsg, IEnumerable<ConversationMessage> conversationHistory, UserContextDto userContextDto);
  IAsyncEnumerable<Result<string>> RouteAndExecuteStreamingAsync(string userMsg, IEnumerable<ConversationMessage> conversationHistory, UserContextDto userContextDto);
}