using FlexNet.Application.DTOs.ChatSession.Request;
using FlexNet.Application.DTOs.ChatSession.Response;
using FlexNet.Domain.Entities;

namespace FlexNet.Application.Interfaces.IRepositories
{
    public interface IChatSessionRepo
    {
        Task<List<ChatSession>> GetAllAsync(int UserID);
        Task<ChatSession?> GetByIdAsync(int SessionID, int UserID);

        Task<ChatSession> AddAsync(ChatSession chatSession);
        Task<ChatSession?> UpdateAsync(ChatSession chatSession);
        Task<bool> DeleteAsync(int SessionID, int UserID);
    }
}
