using FlexNet.Application.DTOs.ChatSession.Request;
using FlexNet.Application.DTOs.ChatSession.Response;
using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Domain.Entities;
using FlexNet.Infrastructure.Data;

namespace FlexNet.Infrastructure.Repositories
{
    class ChatSessionRepo : IChatSessionRepo
    {
        private readonly ApplicationDbContext _context;

        public ChatSessionRepo(ApplicationDbContext ctx)
        {
            _context = ctx;
        }

        public async Task<IEnumerable<CompactChatSessionResponseDto>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<CompleteChatSessionResponseDto> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<User> AddAsync(CreateChatSessionRequestDto chatSession)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }


        public async Task<User> UpdateAsync(UpdateChatSessionsRequestDto chatSession)
        {
            throw new NotImplementedException();
        }
    }
}
