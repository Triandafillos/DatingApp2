﻿using API.DTOs;
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
        public void AddMessage(Message message)
        {
            context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            context.Messages.Remove(message);
        }

        public async Task<Message?> GetMessage(int messageId)
        {
            return await context.Messages.FindAsync(messageId);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = context.Messages.OrderByDescending(m => m.MessageSent).AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(m => m.Recipient.Username == messageParams.Username && m.RecipientDeleted == false),
                "Outbox" => query.Where(m => m.Sender.Username == messageParams.Username && m.SenderDeleted == false),
                _ => query.Where(m => m.Recipient.Username == messageParams.Username && m.DateRead == null && m.RecipientDeleted == false)
            };
            var messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var messages = await context.Messages.Include(m => m.Recipient).ThenInclude(r => r.Photos)
                .Include(m => m.Sender).ThenInclude(s => s.Photos)
                .Where(m => m.Recipient.Username == currentUsername && m.RecipientDeleted == false && m.Sender.Username == recipientUsername ||
                m.Recipient.Username == recipientUsername && m.Sender.Username == currentUsername && m.SenderDeleted == false)
                .OrderBy(m => m.MessageSent).ToListAsync();

            var unreadMessages = messages.Where(m => m.DateRead == null && m.Recipient.Username == currentUsername).ToList();
            if (unreadMessages.Count != 0)
            {
                unreadMessages.ForEach(m => m.DateRead = DateTime.UtcNow);
                await context.SaveChangesAsync();
            }

            return mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }
    }
}