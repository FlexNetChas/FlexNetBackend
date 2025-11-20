using FlexNet.Application.DTOs.AI;
using FlexNet.Application.Models;
using FlexNet.Application.Models.Records;
using FlexNet.Domain.Entities.Schools;

namespace FlexNet.Application.Interfaces.IServices;

public interface ISchoolAdviceGenerator
{
    Task<Result<string>> GenerateAdviceAsync(string userMsg, List<School> schools, UserContextDto userContextDto,
        IEnumerable<ConversationMessage>? recentHistory = null);
    IAsyncEnumerable<Result<string>> GenerateAdviceStreamingAsync(
        string userMsg, 
        List<School> schools, 
        UserContextDto userContextDto,
        IEnumerable<ConversationMessage>? recentHistory = null);
}