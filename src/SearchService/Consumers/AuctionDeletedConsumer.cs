using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService
{
    public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
    {
        public async Task Consume(ConsumeContext<AuctionDeleted> context)
        {
            Console.WriteLine("---> consuming auction deleted:" + context.Message.Id);
            var result = await DB.DeleteAsync<Item>(context.Message.Id);
            if (!result.IsAcknowledged)
                throw new MessageException(typeof(AuctionUpdated),"Problem updating mongodb");

        }
    }
}