using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            userParams.CurrentUsername = User.GetUsername();
            var users = await unitOfWork.UserRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(users);
            return Ok(users);
        }
        
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            MemberDto? user;
            if (username.ToUpper() == User.GetUsername().ToUpper())
            {
                user = await unitOfWork.UserRepository.GetMemberAllPhotosAsync(username);
            }
            else
            {
                user = await unitOfWork.UserRepository.GetMemberAsync(username);
            }
            if (user == null)
            {
                return NotFound();
            }
            return user;
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            if (user == null) return BadRequest("User not found");

            mapper.Map(memberUpdateDto, user);
            if (await unitOfWork.Complete()) return NoContent();
            return BadRequest("Failed to update");

        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            if (user == null) return BadRequest("Cannot find user");

            var result = await photoService.AddPhotoAsync(file);

            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.Url.AbsoluteUri,
                PublicId = result.PublicId,
                IsApproved = false
            };

            user.Photos.Add(photo);

            if (await unitOfWork.Complete())
            {
                return CreatedAtAction(nameof(GetUser), new { username = User.GetUsername() }, mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Error uploading photo");
        }

        [HttpPut("set-main-photo/{photoId:int}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return BadRequest("User not found");

            var photo = user.Photos.FirstOrDefault(p => p.PhotoId == photoId);
            if (photo == null || photo.IsMain || !photo.IsApproved) return BadRequest("Could not set photo to main");

            var currentMain = user.Photos.FirstOrDefault(p => p.IsMain);
            if (currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;

            if (await unitOfWork.Complete()) return NoContent();
            return BadRequest("Error setting photo to main");
        }

        [HttpDelete("delete-photo/{photoId:int}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await unitOfWork.UserRepository.GetUserByUsernameAllPhotos(User.GetUsername());
            if (user == null) return BadRequest("User not found");

            var photo = user.Photos.FirstOrDefault(p => p.PhotoId == photoId);
            if (photo == null || photo.IsMain) return BadRequest("Cannot delete this photo");
            if (photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error != null) return BadRequest(result.Error.Message);
            }
            user.Photos.Remove(photo);

            if (await unitOfWork.Complete()) return Ok();
            return BadRequest("Error deleteing photo");
        }
    }
}