using FlexNet.Application.DTOs.AI;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;

namespace FlexNet.Application.Interfaces.IServices;

public interface INoResultsGenerator
{
   Task<Result<string>> GenerateAsync(string rawMessage, SchoolRequestInfo schoolRequest, UserContextDto userContextDto,
      IEnumerable<ConversationMessage>? recentHistory = null); 
   IAsyncEnumerable<Result<string>> GenerateStreamingAsync(
       string rawMessage, 
       SchoolRequestInfo schoolRequest, 
       UserContextDto userContextDto,
       IEnumerable<ConversationMessage>? recentHistory = null);
}