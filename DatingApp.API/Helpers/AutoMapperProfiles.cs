using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Dtos;
using DatingApp.API.Models;

namespace DatingApp.API.Helpers {
	public class AutoMapperProfiles : Profile {
		public AutoMapperProfiles() {
			this.CreateMap<User, UserForListDto>()
				.ForMember(dest => dest.PhotoUrl,
					opt => opt.MapFrom<string>(src => src.Photos.FirstOrDefault(p => p.IsMain).Url))
				.ForMember(dest => dest.Age, opt => opt.ResolveUsing<int>(dest => dest.DateOfBirth.CalculateAge()));
			this.CreateMap<User, UserForDetailedDto>()
				.ForMember(dest => dest.PhotoUrl,
					opt => opt.MapFrom<string>(src => src.Photos.FirstOrDefault(p => p.IsMain).Url))
				.ForMember(dest => dest.Age,
					opt => opt.ResolveUsing<int>(dest => dest.DateOfBirth.CalculateAge()));
			this.CreateMap<Photo, PhotoForDetailedDto>();
			this.CreateMap<Photo, PhotoForReturnDto>();
			this.CreateMap<PhotoForCreationDto, Photo>();
			this.CreateMap<UserForUpdateDto, User>();
			this.CreateMap<UserForRegisterDto, User>();
			this.CreateMap<MessageForCreationDto, Message>().ReverseMap();
			this.CreateMap<Message, MessageToReturnDto>()
				.ForMember(m => m.SenderPhotoUrl,
					opt => opt.MapFrom(u => u.Sender.Photos.FirstOrDefault(p => p.IsMain).Url))
				.ForMember(m => m.RecipientPhotoUrl,
					opt => opt.MapFrom(u => u.Recipient.Photos.FirstOrDefault(p => p.IsMain).Url));
		}
	}
}
