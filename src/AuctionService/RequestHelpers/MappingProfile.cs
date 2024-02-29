
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;

namespace AuctionService.RequestHelpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // create mapper
            CreateMap<Auction, AuctionDto>().IncludeMembers(x=> x.Item);
            CreateMap<Item, AuctionDto>();
            CreateMap<AuctionDto, Auction>()
                .ForMember(d=> d.Item, o=> o.MapFrom(s=> s));
            CreateMap<CreateAuctionDto, Auction>();

        }
    }
}