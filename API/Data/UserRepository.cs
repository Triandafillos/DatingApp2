using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository(DataContext context, IMapper mapper) : IUserRepository
    {

        public async Task<MemberDto?> GetMemberAllPhotosAsync(string username)
        {
            return await context.Users.IgnoreQueryFilters().ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(m => m.Username!.ToLower() == username.ToLower());
        }

        public async Task<MemberDto?> GetMemberAsync(string username)
        {
            return await context.Users.ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(m => m.Username!.ToLower() == username.ToLower());
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = context.Users.AsQueryable();
            query = query.Where(u => u.UserName != userParams.CurrentUsername);
            if(userParams.Gender != null)
            {
                query = query.Where(u => u.Gender ==  userParams.Gender);
            }
            var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
            var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));
            query = query.Where(u => u.DateOfBirth <= maxDob && u.DateOfBirth >= minDob);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u => u.Created),
                _ => query.OrderByDescending(u => u.LastActive)
            };
            

            return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(mapper.ConfigurationProvider), userParams.PageNumber, userParams.PageSize);
        }

        public async Task<Photo?> GetPhotoByIdAsync(int id)
        {
            return await context.Photos.Include(p => p.AppUser).ThenInclude(u => u.Photos).IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.PhotoId == id);
        }

        public async Task<IEnumerable<PhotoForApprovalDto>> GetPhotosForApprovalAsync()
        {
            return await context.Users.IgnoreQueryFilters().ProjectTo<PhotoForApprovalDto>(mapper.ConfigurationProvider)
                .Select(u => new PhotoForApprovalDto {
                    Username = u.Username,
                    Photos = u.Photos!.Where(p => !p.IsApproved ?? false).ToList()
                }).Where(u => u.Photos!.Count > 0).ToListAsync();
        }

        public async Task<AppUser?> GetUserByIdAsync(int id)
        {
            return await context.Users.FindAsync(id);
        }

        public async Task<AppUser?> GetUserByUsernameAllPhotos(string username)
        {
            return await context.Users.IgnoreQueryFilters().Include(u => u.Photos)
                .SingleOrDefaultAsync(u => u.NormalizedUserName == username.ToUpper());
        }

        public async Task<AppUser?> GetUserByUsernameAsync(string username)
        {
            return await context.Users.Include(u => u.Photos).SingleOrDefaultAsync(u => u.NormalizedUserName == username.ToUpper());
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await context.Users.Include(u => u.Photos).ToListAsync();
        }

        public void Update(AppUser user)
        {
            context.Entry(user).State = EntityState.Modified;
        }
    }
}
