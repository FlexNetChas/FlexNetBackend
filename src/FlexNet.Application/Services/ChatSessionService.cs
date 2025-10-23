using FlexNet.Application.DTOs.ChatMessage.Response;
using FlexNet.Application.DTOs.ChatSession.Request;
using FlexNet.Application.DTOs.ChatSession.Response;
using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Domain.Entities;
using Microsoft.VisualBasic;

namespace FlexNet.Application.Services
{
    class ChatSessionService : IChatSessionService
    {
        private readonly IChatSessionRepo _chatSessionRepo;
        public ChatSessionService(IChatSessionRepo repo)
        {
            _chatSessionRepo = repo;
        }

        async Task<IEnumerable<CompactChatSessionResponseDto>> IChatSessionService.GetAllAsync(int UserID)
        {
            var sessions = await _chatSessionRepo.GetAllAsync(UserID);
            var sessionsDto = sessions.Select(s => new CompactChatSessionResponseDto(
                s.Id ?? -1, s.Summary, s.StartedTime, s.EndedTime
            ));
            return sessionsDto;
        }

        public async Task<CompleteChatSessionResponseDto?> GetByIdAsync(int id, int UserID)
        {
            var entity = await _chatSessionRepo.GetByIdAsync(id, UserID);
            return entity == null ? null : ConvertToCompleteDto(entity);
        }

        public async Task<CompleteChatSessionResponseDto?> CreateAsync(CreateChatSessionRequestDto chatSession, int UserID)
        {
            var entity = new ChatSession
            {
                UserId = UserID,
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

        public async Task<CompleteChatSessionResponseDto?> UpdateAsync(UpdateChatSessionsRequestDto chatSession, int UserID)
        {
            var entity = new ChatSession
            {
                Id = chatSession.SessionID,
                UserId = UserID,
                Summary = chatSession.Summary,
                StartedTime = chatSession.StartedTime,
                EndedTime = chatSession.EndedTime,
                ChatMessages = chatSession.ChatMessages.Select(m =>
                {
                    var chatMessage = new ChatMessage
                    {
                        MessageText = m.MessageText,
                        TimeStamp = m.TimeStamp,
                        LastUpdated = m.LastUpdated
                    };

                    if (m.Id.HasValue)
                        chatMessage.Id = m.Id.Value;

                    return chatMessage;
                }).ToList()
            };

            var updated = await _chatSessionRepo.UpdateAsync(entity);
            return updated != null ? ConvertToCompleteDto(updated) : null;
        }

        public async Task<bool> DeleteAsync(int id, int UserID)
        {
            return await _chatSessionRepo.DeleteAsync(id, UserID);
        }

        private CompleteChatSessionResponseDto ConvertToCompleteDto(ChatSession session)
        {
            return new CompleteChatSessionResponseDto(
                session.Id ?? -1,
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
