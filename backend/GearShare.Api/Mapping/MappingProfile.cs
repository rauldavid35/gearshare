using System.Linq;
using AutoMapper;
using GearShare.Api.Domain.Entities;
using GearShare.Api.DTOs.Items;
using GearShare.Api.DTOs.Listings;
using GearShare.Api.DTOs.Bookings;


namespace GearShare.Api.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Item
            CreateMap<Item, ItemDto>()
                .ForMember(d => d.Images, m => m.MapFrom(s =>
                    s.Images.OrderBy(x => x.SortOrder).Select(x => x.RelativePath).ToList()))
                .ForMember(d => d.ListingsCount, m => m.MapFrom(s => s.Listings.Count));
            CreateMap<CreateItemRequest, Item>();
            CreateMap<UpdateItemRequest, Item>();

            // Listing -> ListingDto (cu titlu + cover din Item)
            CreateMap<Listing, ListingDto>()
                .ForCtorParam("Id", o => o.MapFrom(s => s.Id))
                .ForCtorParam("ItemId", o => o.MapFrom(s => s.ItemId))
                .ForCtorParam("ItemTitle", o => o.MapFrom(s => s.Item.Title))
                .ForCtorParam("CoverImage", o => o.MapFrom(s =>
                     s.Item.Images.OrderBy(ii => ii.SortOrder)
                                  .Select(ii => ii.RelativePath)
                                  .FirstOrDefault()))
                .ForCtorParam("PricePerDay", o => o.MapFrom(s => s.PricePerDay))
                .ForCtorParam("Deposit", o => o.MapFrom(s => s.Deposit))
                .ForCtorParam("LocationCity", o => o.MapFrom(s => s.LocationCity))
                .ForCtorParam("LocationLat", o => o.MapFrom(s => s.LocationLat))
                .ForCtorParam("LocationLng", o => o.MapFrom(s => s.LocationLng))
                .ForCtorParam("Active", o => o.MapFrom(s => s.Active));

            CreateMap<Booking, BookingDto>()
                .ForCtorParam("ListingTitle", opt => opt.MapFrom(b => b.Listing.Item.Title));

            CreateMap<CreateListingRequest, Listing>();
            CreateMap<UpdateListingRequest, Listing>();
        }
    }
}
