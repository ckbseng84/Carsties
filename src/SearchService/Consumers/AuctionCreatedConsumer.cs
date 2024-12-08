using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService
{
    //create mass transit consumer for AuctionCreated
    public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
    {
        private readonly IMapper _mapper;

        public AuctionCreatedConsumer(IMapper mapper)
        {
            _mapper = mapper;
        }
        public async Task Consume(ConsumeContext<AuctionCreated> context)
        {
            Console.WriteLine("---> consuming auction created:" + context.Message.Id);
            var item = _mapper.Map<Item>(context.Message);
            //is an example
            if (item.Model == "Foo") throw new ArgumentException("Cannot sell cars with name of foo");
            await item.SaveAsync();
        }
    }
}