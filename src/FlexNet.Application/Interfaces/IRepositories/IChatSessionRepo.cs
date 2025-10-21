using FlexNet.Application.DTOs.ChatSession.Request;
using FlexNet.Application.DTOs.ChatSession.Response;
using FlexNet.Domain.Entities;

namespace FlexNet.Application.Interfaces.IRepositories
{
    public interface IChatSessionRepo
    {
        Task<IEnumerable<ChatSession>> GetAllAsync();
        Task<ChatSession> GetByIdAsync(int id);

        Task<ChatSession> AddAsync(ChatSession chatSession);
        Task<ChatSession> UpdateAsync(ChatSession chatSession);
        Task<bool> DeleteAsync(int id);
    }
}
