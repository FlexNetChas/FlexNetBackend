using FlexNet.Application.DTOs.ChatMessage.Response;
using FlexNet.Application.DTOs.ChatSession.Request;
using FlexNet.Application.DTOs.ChatSession.Response;
using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Application.Interfaces.IServices;
using FlexNet.Domain.Entities;

namespace FlexNet.Application.Services
{
    public class ChatSessionService : IChatSessionService
    {
        private readonly IChatSessionRepo _chatSessionRepo;
        private readonly IUserContextService _userContextService;
        public ChatSessionService(IChatSessionRepo repo, IUserContextService userContextService)
        {
            _chatSessionRepo = repo;
            _userContextService = userContextService;
        }

        public async Task<IEnumerable<CompactChatSessionResponseDto>> GetAllAsync()
        {
            var userID = _userContextService.GetCurrentUserId();
            var sessions = await _chatSessionRepo.GetAllAsync(userID);
            var sessionsDto = sessions.Select(s => new CompactChatSessionResponseDto(
                s.Id ?? -1, s.Summary, s.StartedTime, s.EndedTime
            ));
            return sessionsDto;
        }

        public async Task<CompleteChatSessionResponseDto?> GetByIdAsync(int sessionID)
        {
            var userID = _userContextService.GetCurrentUserId();
            var entity = await _chatSessionRepo.GetByIdAsync(sessionID, userID);

            if (entity is null)
            {
                throw new KeyNotFoundException("Chat session couldn't be found. Please try again!");
            }

            return ConvertToCompleteDto(entity);
        }

        public async Task<CompleteChatSessionResponseDto?> CreateAsync(CreateChatSessionRequestDto chatSession)
        {
            var userID = _userContextService.GetCurrentUserId();
            var entity = new ChatSession
            {
                UserId = userID,
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

        public async Task<CompleteChatSessionResponseDto?> UpdateAsync(UpdateChatSessionsRequestDto chatSession)
        {
            var userID = _userContextService.GetCurrentUserId();
            var entity = new ChatSession
            {
                Id = chatSession.SessionID,
                UserId = userID,
                Summary = chatSession.Summary,
                StartedTime = chatSession.StartedTime,
                EndedTime = chatSession.EndedTime,
                ChatMessages = chatSession.ChatMessages.Select(m =>
                {
                    var chatMessage = new ChatMessage
                    {
                        MessageText = m.MessageText,
                        TimeStamp = m.TimeStamp,
                        LastUpdated = m.LastUpdated,
                        Role = m.Role
                    };

                    if (m.Id.HasValue)
                        chatMessage.Id = m.Id.Value;

                    return chatMessage;
                }).ToList()
            };

            var updated = await _chatSessionRepo.UpdateAsync(entity);

            if (updated is null)
            {
                throw new KeyNotFoundException($"Chat session couldn't be found. Please try again!");
            }

            return ConvertToCompleteDto(updated);
        }

        public async Task<bool> DeleteAsync(int sessionID)
        {
            var userID = _userContextService.GetCurrentUserId();
            var result = await _chatSessionRepo.DeleteAsync(sessionID, userID);

            if (!result)
            {
                throw new KeyNotFoundException("Chat session is already deleted or couldn't be found");
            }

            return result;
        }

        public async Task<CompleteChatSessionResponseDto?> EndSessionAsync(int sessionID)
        {
            var userID = _userContextService.GetCurrentUserId();
            var session = await _chatSessionRepo.GetByIdAsync(sessionID, userID);
            if (session == null) throw new KeyNotFoundException("Chat session couldn't be found");
            if(session.EndedTime.HasValue) throw new InvalidOperationException("Chat session is already ended");
            session.EndedTime = DateTime.UtcNow;
            var updated = await _chatSessionRepo.UpdateAsync(session);
            if(updated == null) throw new KeyNotFoundException("Chat session couldn't be updated");
            return ConvertToCompleteDto(updated);
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
                    m.Id, m.MessageText, m.TimeStamp, m.LastUpdated, m.Role
                )).ToList()
            );
        }
    }
}
