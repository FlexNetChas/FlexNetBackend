using FlexNet.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexNet.Application.Interfaces.IRepositories
{
    public interface IChatSessionRepo
    {
        Task<IEnumerable<ChatSession>> GetAllAsync();
        Task<ChatSession> GetByIdAsync(int id);

        Task<User> AddAsync(ChatSession chatSession);
        Task<User> UpdateAsync(ChatSession chatSession);
        Task<bool> DeleteAsync(int id);
    }
}
