using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers
{
    public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
    {
        private readonly IMapper _mapper;

        public AuctionUpdatedConsumer(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task Consume(ConsumeContext<AuctionUpdated> context)
        {
            Console.WriteLine("---> consuming auction updated:" + context.Message.Id);
            var result = await DB.Update<Item>()
                .MatchID(context.Message.Id)
                .ModifyOnly(x => new  
                { 
                    x.Model,
                    x.Color,
                    x.Make,
                    x.Year,
                    x.Mileage,
                    
                }, _mapper.Map<Item>(context.Message))
                .ExecuteAsync();
            if (!result.IsAcknowledged)
                throw new MessageException(typeof(AuctionUpdated),"Problem updating mongodb");
        }
    }
}