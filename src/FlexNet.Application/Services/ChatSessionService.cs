using FlexNet.Application.DTOs.ChatMessage.Response;
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
            var sessions = await _chatSessionRepo.GetAllAsync();
            var sessionsDto = sessions.Select(s => new CompactChatSessionResponseDto(
                s.Id, s.Summary, s.StartedTime, s.EndedTime
            ));
            return sessionsDto;
        }

        public async Task<CompleteChatSessionResponseDto> GetByIdAsync(int id)
        {
            var entity = await _chatSessionRepo.GetByIdAsync(id);
            return entity == null ? null : ConvertToCompleteDto(entity);
        }

        public async Task<CompleteChatSessionResponseDto> CreateAsync(CreateChatSessionRequestDto chatSession)
        {
            var entity = new ChatSession
            {
                UserId = chatSession.userID,
                Summary = chatSession.Summary,
                StartedTime = chatSession.StartedTime,
                EndedTime = chatSession.EndedTime,
                ChatMessages = chatSession.ChatMessages.Select(m => new ChatMessage
                {
                    MessageText = m.MessageText,
                    TimeStamp = m.TimeStamp,
                    LastUpdated = m.LastUpdated
                }).ToList()
            };

            var created = await _chatSessionRepo.AddAsync(entity);
            return ConvertToCompleteDto(created);
        }

        public async Task<CompleteChatSessionResponseDto> UpdateAsync(UpdateChatSessionsRequestDto chatSession)
        {
            var entity = new ChatSession
            {
                Id = chatSession.Id,
                Summary = chatSession.Summary,
                StartedTime = chatSession.StartedTime,
                EndedTime = chatSession.EndedTime,
                ChatMessages = chatSession.ChatMessages.Select(m => new ChatMessage
                {
                    MessageText = m.MessageText,
                    TimeStamp = m.TimeStamp,
                    LastUpdated = m.LastUpdated
                }).ToList()
            };

            var updated = await _chatSessionRepo.UpdateAsync(entity);
            return updated != null ? ConvertToCompleteDto(updated) : null;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _chatSessionRepo.DeleteAsync(id);
        }

        private CompleteChatSessionResponseDto ConvertToCompleteDto(ChatSession session)
        {
            return new CompleteChatSessionResponseDto(
                session.Id,
                session.UserId,
                session.Summary,
                session.StartedTime,
                session.EndedTime,
                session.ChatMessages.Select(m => new ChatMessageResponseDto(
                    m.Id, m.MessageText, m.TimeStamp, m.LastUpdated
                )).ToList()
            );
        }
    }
}
