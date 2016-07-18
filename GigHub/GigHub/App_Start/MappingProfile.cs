using AutoMapper;
using GigHub.Dtos;
using GigHub.Models;

namespace GigHub.App_Start
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            Mapper.CreateMap<ApplicationUser, UserDto>();
            Mapper.CreateMap<Genre, GenreDto>();
            Mapper.CreateMap<Gig, GigDto>();
            Mapper.CreateMap<Notification, NotificationDto>();
        }

        //Map DTOs to their respective classes
        protected override void Configure()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ApplicationUser, UserDto>();
                cfg.CreateMap<Genre, GenreDto>();
                cfg.CreateMap<Gig, GigDto>();
                cfg.CreateMap<Notification, NotificationDto>();
            });
        }
    }
}