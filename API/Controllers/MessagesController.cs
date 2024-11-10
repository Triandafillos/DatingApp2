using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController(IUnitOfWork unitOfWork, IMapper mapper) : BaseApiController
    {
        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUsername();
            if (username == createMessageDto.RecipientUsername.ToLower()) return BadRequest("You cannot message yourself");

            var sender = await unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
            if (sender == null || recipient == null || recipient.UserName == null || sender.UserName == null) 
                return BadRequest("Cannot send the message");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                Content = createMessageDto.Content,
                RecipientUsername = recipient.UserName,
                SenderUsername = sender.UserName
            };
            unitOfWork.MessageRepository.AddMessage(message);
            if (await unitOfWork.Complete()) return Ok(mapper.Map<MessageDto>(message));
            return BadRequest("Failed to create message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();

            var messages = await unitOfWork.MessageRepository.GetMessagesForUser(messageParams);
            Response.AddPaginationHeader(messages);
            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUsername = User.GetUsername();

            return Ok(await unitOfWork.MessageRepository.GetMessageThread(username, currentUsername));
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();
            var message = await unitOfWork.MessageRepository.GetMessage(id);
            if (message == null) return BadRequest("Message not found");
            if (message.SenderUsername != username && message.RecipientUsername != username) return Forbid();

            if (message.SenderUsername == username) message.SenderDeleted = true;
            if (message.RecipientUsername == username) message.RecipientDeleted = true;
            if (message.SenderDeleted == true && message.RecipientDeleted == true) unitOfWork.MessageRepository.DeleteMessage(message);

            if (await unitOfWork.Complete()) return Ok();

            return BadRequest("Error deleting message");
        }
    }
}
