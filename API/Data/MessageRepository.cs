using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository(DataContext context, IMapper mapper) : IMessageRepository
    {
        public void AddGroup(Group group)
        {
            context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            context.Messages.Remove(message);
        }

        public async Task<Connection?> GetConnection(string connectionId)
        {
            return await context.Connections.FindAsync(connectionId);
        }

        public async Task<Group?> GetGroupForConnection(string connectionId)
        {
            return await context.Groups.Include(g => g.Connections).Where(g => g.Connections
            .Any(c => c.ConnectionId == connectionId)).FirstOrDefaultAsync();
        }

        public async Task<Message?> GetMessage(int messageId)
        {
            return await context.Messages.FindAsync(messageId);
        }

        public async Task<Group?> GetMessageGroup(string groupName)
        {
            return await context.Groups.Include(g => g.Connections).FirstOrDefaultAsync(c => c.Name ==  groupName);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = context.Messages.OrderByDescending(m => m.MessageSent).AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(m => m.Recipient.UserName == messageParams.Username && m.RecipientDeleted == false),
                "Outbox" => query.Where(m => m.Sender.UserName == messageParams.Username && m.SenderDeleted == false),
                _ => query.Where(m => m.Recipient.UserName == messageParams.Username && m.DateRead == null && m.RecipientDeleted == false)
            };
            var messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var messages = await context.Messages.Where(m => m.Recipient.UserName == currentUsername && m.RecipientDeleted == false && m.Sender.UserName == recipientUsername ||
                m.Recipient.UserName == recipientUsername && m.Sender.UserName == currentUsername && m.SenderDeleted == false)
                .OrderBy(m => m.MessageSent).ProjectTo<MessageDto>(mapper.ConfigurationProvider).ToListAsync();

            var unreadMessages = messages.Where(m => m.DateRead == null && m.RecipientUsername == currentUsername).ToList();
            if (unreadMessages.Count != 0)
            {
                unreadMessages.ForEach(m => m.DateRead = DateTime.UtcNow);
                await context.SaveChangesAsync();
            }

            return messages;
        }

        public void RemoveConnection(Connection connection)
        {
            context.Connections.Remove(connection);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }
    }
}
