using FlexNet.Application.DTOs.ChatSession.Request;
using FlexNet.Application.DTOs.ChatSession.Response;

namespace FlexNet.Application.Interfaces.IServices
{
    public interface IChatSessionService
    {
        Task<IEnumerable<CompactChatSessionResponseDto>> GetAllAsync();
        Task<CompleteChatSessionResponseDto?> GetByIdAsync(int sessionID);
        Task<CompleteChatSessionResponseDto?> CreateAsync(CreateChatSessionRequestDto chatSession);
        Task<CompleteChatSessionResponseDto?> UpdateAsync(UpdateChatSessionsRequestDto chatSession);
        Task<CompleteChatSessionResponseDto?> EndSessionAsync(int sessionId);
        Task<bool> DeleteAsync(int sessionID);
    }
}
