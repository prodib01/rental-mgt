using AutoMapper;
using RentalManagementSystem.Models;
using RentalManagementSystem.DTOs;
using RentalManagementSystem.ViewModels;

namespace RentalManagementSystem
{
    public class MappingProfile : AutoMapper.Profile // Explicitly reference AutoMapper's Profile
    {
        public MappingProfile()
        {
            CreateMap<Request, RequestDto>()
                .ForMember(dest => dest.TenantName, 
                    opt => opt.MapFrom(src => src.Tenant.FullName))
                .ForMember(dest => dest.HouseNumber,
                    opt => opt.MapFrom(src => src.Tenant.House.HouseNumber)); // Adding mapping for HouseNumber

            CreateMap<CreateRequestDto, Request>();
            CreateMap<RequestDto, RequestViewModel>();
        }
    }
}
