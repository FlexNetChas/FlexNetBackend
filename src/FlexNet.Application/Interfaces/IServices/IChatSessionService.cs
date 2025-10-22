using FlexNet.Application.DTOs.ChatSession.Request;
using FlexNet.Application.DTOs.ChatSession.Response;

namespace FlexNet.Application.Interfaces.IServices
{
    public interface IChatSessionService
    {
        Task<IEnumerable<CompactChatSessionResponseDto>> GetAllAsync(int UserID);
        Task<CompleteChatSessionResponseDto?> GetByIdAsync(int SessionID, int UserID);
        Task<CompleteChatSessionResponseDto?> CreateAsync(CreateChatSessionRequestDto chatSession, int UserID);
        Task<CompleteChatSessionResponseDto?> UpdateAsync(UpdateChatSessionsRequestDto chatSession, int UserID);
        Task<bool> DeleteAsync(int SessionID, int UserID);
    }
}
