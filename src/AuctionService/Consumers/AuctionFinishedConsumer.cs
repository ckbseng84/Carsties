using AuctionService.Data;
using AuctionService.Entities;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers
{
    //receive when auction is finished
    public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
    {
        private readonly AuctionDbContext _dbContext;

        public AuctionFinishedConsumer(AuctionDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task Consume(ConsumeContext<AuctionFinished> context)
        {
            Console.WriteLine("--> consuming auction finished");

            //get auction
            var auction = await _dbContext.Auctions.FindAsync(context.Message.AuctionId);

            //if auction is sold, set winner and sold amount
            if(context.Message.ItemSold)
            {
                auction.Winner = context.Message.Winner;
                auction.SoldAmount = context.Message.Amount;
            }

            //if sold amount > reservice price, update status to finish, else update to Reserve not met
            auction.Status = auction.SoldAmount > auction.ReservePrice
                ? Status.Finished : Status.ReserveNotMet;

            //save auction
            await _dbContext.SaveChangesAsync();
        }
    }
}