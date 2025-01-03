using AutoMapper;
using RentalManagementSystem.Models;
using RentalManagementSystem.DTOs;
using RentalManagementSystem.ViewModels;

namespace RentalManagementSystem
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Request, RequestDto>()
                .ForMember(dest => dest.TenantName, 
                    opt => opt.MapFrom(src => src.Tenant.FullName));

            CreateMap<CreateRequestDto, Request>();
            CreateMap<RequestDto, RequestViewModel>();
        }
    }
}