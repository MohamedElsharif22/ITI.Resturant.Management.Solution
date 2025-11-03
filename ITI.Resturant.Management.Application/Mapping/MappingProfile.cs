using AutoMapper;
using ITI.Resturant.Management.Application.DTOs.Account;
using ITI.Resturant.Management.Application.DTOs.Menu;
using ITI.Resturant.Management.Application.DTOs.Order;
using ITI.Resturant.Management.Domain.Entities.Menu;
using ITI.Resturant.Management.Domain.Entities.Order_;
using ITI.Resturant.Management.Domain.Identity;

namespace ITI.Resturant.Management.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Menu mappings
            CreateMap<MenuItem, MenuItemDto>();
            CreateMap<MenuItemDto, MenuItem>();

            // Order mappings
            CreateMap<Order, OrderDto>();
            CreateMap<OrderDto, Order>();
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(d => d.MenuItemName, o => o.MapFrom(s => s.MenuItem.Name));
            CreateMap<OrderItemDto, OrderItem>();

            // User/Account mappings
            CreateMap<RegisterDto, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName));

            CreateMap<ApplicationUser, RegisterDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                // do not map password back
                .ForMember(dest => dest.Password, opt => opt.Ignore());
        }
    }
}
