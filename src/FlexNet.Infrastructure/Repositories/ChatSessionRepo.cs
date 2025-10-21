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

        public async Task<IEnumerable<ChatSession>> GetAllAsync()
        {
            return await _context.ChatSessions
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ChatSession?> GetByIdAsync(int id)
        {
            return await _context.ChatSessions
                .Include(s => s.ChatMessages)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<ChatSession> AddAsync(ChatSession entity)
        {
            _context.ChatSessions.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<ChatSession?> UpdateAsync(ChatSession entity)
        {
            var existing = await _context.ChatSessions
                .Include(s => s.ChatMessages)
                .FirstOrDefaultAsync(s => s.Id == entity.Id);

            if (existing == null)
                return null;

            existing.Summary = entity.Summary;
            existing.StartedTime = entity.StartedTime;
            existing.EndedTime = entity.EndedTime;

            //_context.ChatMessage.RemoveRange(existing.ChatMessages);
            //existing.ChatMessages = entity.ChatMessages;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.ChatSessions.FindAsync(id);
            if (existing == null)
                return false;

            _context.ChatSessions.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
