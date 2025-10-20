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
            // Item - CRITICAL FIX: Force enumeration with ToList() on source
            CreateMap<Item, ItemDto>()
                .ForMember(d => d.Images, m => m.MapFrom(s =>
                    s.Images == null || !s.Images.Any()
                        ? new List<string>()
                        : s.Images.ToList()  // Force enumeration FIRST
                                  .OrderBy(x => x.SortOrder)
                                  .Select(x => x.RelativePath)
                                  .ToList()))
                .ForMember(d => d.ListingsCount, m => m.MapFrom(s => 
                    s.Listings == null ? 0 : s.Listings.Count));
            
            CreateMap<CreateItemRequest, Item>();
            CreateMap<UpdateItemRequest, Item>();

            // Rest of your mappings...
            CreateMap<Listing, ListingDto>()
                .ForCtorParam("Id", o => o.MapFrom(s => s.Id))
                .ForCtorParam("ItemId", o => o.MapFrom(s => s.ItemId))
                .ForCtorParam("ItemTitle", o => o.MapFrom(s => s.Item != null ? s.Item.Title : ""))
                .ForCtorParam("CoverImage", o => o.MapFrom(s =>
                     s.Item != null && s.Item.Images != null && s.Item.Images.Any()
                         ? s.Item.Images.OrderBy(ii => ii.SortOrder)
                                      .Select(ii => ii.RelativePath)
                                      .FirstOrDefault()
                         : null))
                .ForCtorParam("PricePerDay", o => o.MapFrom(s => s.PricePerDay))
                .ForCtorParam("Deposit", o => o.MapFrom(s => s.Deposit))
                .ForCtorParam("LocationCity", o => o.MapFrom(s => s.LocationCity))
                .ForCtorParam("LocationLat", o => o.MapFrom(s => s.LocationLat))
                .ForCtorParam("LocationLng", o => o.MapFrom(s => s.LocationLng))
                .ForCtorParam("Active", o => o.MapFrom(s => s.Active));

            CreateMap<Booking, BookingDto>()
                .ForCtorParam("ListingTitle", opt => opt.MapFrom(b => 
                    b.Listing != null && b.Listing.Item != null ? b.Listing.Item.Title : ""));

            CreateMap<CreateListingRequest, Listing>();
            CreateMap<UpdateListingRequest, Listing>();
        }
    }
}