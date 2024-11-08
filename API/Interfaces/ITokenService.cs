using API.Entities;
using Microsoft.AspNetCore.Identity;

namespace API.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateToken(AppUser user);
    }
}
