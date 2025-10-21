using FlexNet.Application.DTOs.ChatSession.Request;
using FlexNet.Application.DTOs.ChatSession.Response;
using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Domain.Entities;

namespace FlexNet.Application.Services
{
    class ChatSessionService : IChatSessionService
    {
        private readonly IChatSessionRepo _chatSessionRepo;
        public ChatSessionService(IChatSessionRepo repo)
        {
            _chatSessionRepo = repo;       
        }

        async Task<IEnumerable<CompactChatSessionResponseDto>> IChatSessionService.GetAllAsync()
        {
            return await _chatSessionRepo.GetAllAsync();
        }

        public async Task<CompleteChatSessionResponseDto> GetByIdAsync(int id)
        {
            return await _chatSessionRepo.GetByIdAsync(id);
        }

        public async Task<User> CreateAsync(CreateChatSessionRequestDto chatSession)
        {
            return await _chatSessionRepo.AddAsync(chatSession);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _chatSessionRepo.DeleteAsync(id);
        }

        public async Task<User> UpdateAsync(UpdateChatSessionsRequestDto chatSession)
        {
            return await _chatSessionRepo.UpdateAsync(chatSession);
        }


    }
}
