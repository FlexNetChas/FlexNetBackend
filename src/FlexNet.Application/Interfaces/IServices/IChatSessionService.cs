using FlexNet.Application.DTOs.ChatSession.Request;
using FlexNet.Application.DTOs.ChatSession.Response;
using FlexNet.Domain.Entities;

namespace FlexNet.Application.Interfaces.IServices
{
    public interface IChatSessionService
    {
        Task<IEnumerable<CompactChatSessionResponseDto>> GetAllAsync();
        Task<CompleteChatSessionResponseDto> GetByIdAsync(int id);

        Task<User> CreateAsync(CreateChatSessionRequestDto chatSession);
        Task<User> UpdateAsync(UpdateChatSessionsRequestDto chatSession);
        Task<bool> DeleteAsync(int id);
    }
}
