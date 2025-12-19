using AutoMapper;
using DAL.Models;
using WebApi.DTOs.Auth;
using WebApi.DTOs.Components;
using WebApi.DTOs.ComponentTypes;
using WebApi.DTOs.Configurations;
using WebApi.DTOs.Images;
using WebApi.DTOs.Compatibilities;

namespace WebApi.MappingProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserRegisterDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordSalt, opt => opt.Ignore())
                .ForMember(dest => dest.ApiLogs, opt => opt.Ignore())
                .ForMember(dest => dest.CarConfigurations, opt => opt.Ignore());

            CreateMap<ComponentCreateDto, Component>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ComponentType, opt => opt.Ignore())
                .ForMember(dest => dest.Image, opt => opt.Ignore())
                .ForMember(dest => dest.CarConfigurationComponents, opt => opt.Ignore())
                .ForMember(dest => dest.ComponentCompatibilityComponents, opt => opt.Ignore())
                .ForMember(dest => dest.ComponentCompatibilityCompatibleWithComponents, opt => opt.Ignore());

            CreateMap<ComponentUpdateDto, Component>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ComponentType, opt => opt.Ignore())
                .ForMember(dest => dest.Image, opt => opt.Ignore())
                .ForMember(dest => dest.CarConfigurationComponents, opt => opt.Ignore())
                .ForMember(dest => dest.ComponentCompatibilityComponents, opt => opt.Ignore())
                .ForMember(dest => dest.ComponentCompatibilityCompatibleWithComponents, opt => opt.Ignore());

            CreateMap<Component, ComponentResponseDto>()
                .ForMember(dest => dest.ComponentTypeName,
                    opt => opt.MapFrom(src =>
                        src.ComponentType != null ? src.ComponentType.Name : ""))
                .ForMember(dest => dest.ImageUrl,
                    opt => opt.MapFrom(src =>
                        src.Image != null ? src.Image.StoragePathOrUrl : null));

            CreateMap<ComponentTypeCreateDto, ComponentType>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Components, opt => opt.Ignore());

            CreateMap<ComponentTypeUpdateDto, ComponentType>()
                .ForMember(dest => dest.Components, opt => opt.Ignore());

            CreateMap<ComponentType, ComponentTypeResponseDto>();


            CreateMap<CarConfiguration, ConfigurationResponseDto>();

            CreateMap<CarConfiguration, ConfigurationDetailsResponseDto>()
                .ForMember(dest => dest.Components, opt => opt.Ignore());


            CreateMap<ImageCreateDto, Image>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UploadedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Components, opt => opt.Ignore());

            CreateMap<Image, ImageResponseDto>();


            CreateMap<ComponentCompatibility, CompatibilityResponseDto>();
        }
    }
}
