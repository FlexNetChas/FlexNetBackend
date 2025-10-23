using FlexNet.Application.Interfaces.IRepositories;
using FlexNet.Domain.Entities;
using FlexNet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlexNet.Infrastructure.Repositories
{
    class ChatSessionRepo : IChatSessionRepo
    {
        private readonly ApplicationDbContext _context;

        public ChatSessionRepo(ApplicationDbContext ctx)
        {
            _context = ctx;
        }

        public async Task<List<ChatSession>> GetAllAsync(int UserID)
        {
            var usersChatSessions = await _context.ChatSessions
                .Where(s => s.UserId == UserID)
                .ToListAsync();

            return usersChatSessions;
        }

        public async Task<ChatSession?> GetByIdAsync(int SessionID, int UserID)
        {
            return await _context.ChatSessions
                .Where(s => s.UserId == UserID && s.Id == SessionID)
                .Include(s => s.ChatMessages)
                .FirstOrDefaultAsync();
        }

        public async Task<ChatSession> AddAsync(ChatSession entity)
        {
            _context.ChatSessions.Add(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<ChatSession?> UpdateAsync(ChatSession entity)
        {
            var chatSession = await _context.ChatSessions
                .Include(s => s.ChatMessages)
                .FirstOrDefaultAsync(s => s.Id == entity.Id && s.UserId == entity.UserId);

            if (chatSession == null)
                return null;

            chatSession.Summary = entity.Summary;
            chatSession.StartedTime = entity.StartedTime;
            chatSession.EndedTime = entity.EndedTime;

            //Might not need this depending on how/if we let users update old chat messages
            foreach (var message in entity.ChatMessages)
            {
                if (message.Id == 0)
                {
                    chatSession.ChatMessages.Add(message);
                }
                else
                {
                    var existingMessage = chatSession.ChatMessages
                        .FirstOrDefault(m => m.Id == message.Id);

                    if (existingMessage != null)
                    {
                        existingMessage.MessageText = message.MessageText;
                        existingMessage.TimeStamp = message.TimeStamp;
                        existingMessage.LastUpdated = message.LastUpdated;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return chatSession;
        }

        public async Task<bool> DeleteAsync(int id, int UserID)
        {
            var chatSession = await _context.ChatSessions
                .Where(s => s.UserId == UserID && s.Id == id)
                .FirstOrDefaultAsync();

            if (chatSession == null)
                return false;

            _context.ChatSessions.Remove(chatSession);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
