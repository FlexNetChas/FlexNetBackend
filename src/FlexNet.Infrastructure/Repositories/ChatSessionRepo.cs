using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Domain.Entities;
using FlexNet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexNet.Infrastructure.Repositories
{
    class ChatSessionRepo : IChatSessionRepo
    {
        private readonly ApplicationDbContext _context;

        public ChatSessionRepo(ApplicationDbContext ctx)
        {
            _context = ctx;
        }

        public Task<IEnumerable<ChatSession>> GetAllAsync()
        {
            
            throw new NotImplementedException();
        }

        public Task<ChatSession> GetByIdAsync(int id)
        {

            throw new NotImplementedException();
        }

        public Task<User> AddAsync(ChatSession chatSession)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<User> UpdateAsync(ChatSession chatSession)
        {
            throw new NotImplementedException();
        }
    }
}
