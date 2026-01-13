using AutoMapper;
using CarConfigurator_WebApp.ViewModels;
using DAL.Models;

namespace CarConfigurator_WebApp.MappingProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //Za komponente
            CreateMap<Component, ComponentListItemVM>()
                .ForMember(d => d.ComponentTypeName, opt => opt.Ignore());

            CreateMap<Component, ComponentEditVM>()
                .ForMember(d => d.ComponentTypes, opt => opt.Ignore());

            CreateMap<Component, ComponentDeleteVM>()
                .ForMember(d => d.ComponentTypeName, opt => opt.Ignore());

            CreateMap<ComponentCreateVM, Component>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.ComponentType, opt => opt.Ignore())
                .ForMember(d => d.Image, opt => opt.Ignore())
                .ForMember(d => d.CarConfigurationComponents, opt => opt.Ignore());

            CreateMap<ComponentEditVM, Component>()
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.ComponentType, opt => opt.Ignore())
                .ForMember(d => d.Image, opt => opt.Ignore())
                .ForMember(d => d.CarConfigurationComponents, opt => opt.Ignore());

            //Za tip komponenti
            CreateMap<ComponentType, ComponentTypeListItemVM>();
            CreateMap<ComponentType, ComponentTypeEditVM>();
            CreateMap<ComponentType, ComponentTypeDeleteVM>();

            CreateMap<ComponentTypeCreateVM, ComponentType>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.Components, opt => opt.Ignore());

            CreateMap<ComponentTypeEditVM, ComponentType>()
                .ForMember(d => d.Components, opt => opt.Ignore());

            // Za kompatibilnosti
            CreateMap<ComponentCompatibility, CompatibilityListItemVM>()
                .ForMember(d => d.ComponentName, opt => opt.Ignore())
                .ForMember(d => d.CompatibleWithComponentName, opt => opt.Ignore());

            // Za ITEMS (Admin/User)
            CreateMap<Component, ItemsListItemVM>()
                .ForMember(d => d.ComponentTypeName, opt => opt.Ignore());

            CreateMap<Component, ItemDetailsVM>()
                .ForMember(d => d.ComponentTypeName, opt => opt.Ignore());

            // Za konfiguracije (Admin/User)
            CreateMap<Component, ConfigurationComponentItemVM>()
                .ForMember(d => d.ComponentId, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.ComponentTypeName, opt => opt.Ignore());

            // Za profile admin i user
            CreateMap<User, AdminProfileVM>();
        }
    }
}
