using AutoMapper;
using SPA_Comments.DTO;
using SPA_Comments.Models;

namespace CommentSystem.Api.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Comment, CommentResponseDto>()
                .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => src.Replies));

            CreateMap<CommentCreateDto, Comment>()
                .ForMember(dest => dest.FilePath, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        }
    }
}