using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, MemberDto>()
                .ForMember(u => u.Age, m => m.MapFrom(d => d.DateOfBirth.CalculateAge()))
                .ForMember(u => u.PhotoUrl, m => m.MapFrom(d => d.Photos.FirstOrDefault(p => p.IsMain)!.Url));
            CreateMap<Photo, PhotoDto>()
                .ForMember(p => p.PhotoId, m => m.MapFrom(d => d.PhotoId));
            CreateMap<MemberUpdateDto, AppUser>();
            CreateMap<RegisterDto, AppUser>();
            CreateMap<string, DateOnly>().ConvertUsing(s => DateOnly.Parse(s));
            CreateMap<Message, MessageDto>()
                .ForMember(m => m.SenderPhotoUrl, o => o.MapFrom(m => m.Sender.Photos.FirstOrDefault(p => p.IsMain)!.Url))
                .ForMember(m => m.RecipientPhotoUrl, o => o.MapFrom(m => m.Recipient.Photos.FirstOrDefault(p => p.IsMain)!.Url));
            CreateMap<DateTime, DateTime>().ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
            CreateMap<DateTime?, DateTime?>().ConvertUsing(d => d.HasValue ? DateTime.SpecifyKind(d.Value, DateTimeKind.Utc) : null);
            CreateMap<AppUser, PhotoForApprovalDto>();
        }
    }
}
