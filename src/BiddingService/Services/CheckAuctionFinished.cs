using BiddingService.Models;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService.Services
{
    //background service
    public class CheckAuctionFinished : BackgroundService
    {
        private readonly ILogger<CheckAuctionFinished> _logger;
        private readonly IServiceProvider _services;

        public CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting check for finished auctions");
            
            //register action to be trigger on stoppingToken
            stoppingToken.Register(() => _logger.LogInformation("===> auction check is stoping"));//log information if service is stop
            

            //when is not cancal
            while(!stoppingToken.IsCancellationRequested)
            {
                //check auctions
                await CheckAuctions(stoppingToken);
                //buffer for waiting
                await Task.Delay(5000, stoppingToken);
            }
        }

        private async Task CheckAuctions(CancellationToken stoppingToken)
        {
            // get auctions
            var finishedAuctions = await DB.Find<Auction>()
                .Match(x=> x.AuctionEnd <= DateTime.UtcNow)
                .Match(x=> !x.Finished)
                .ExecuteAsync(stoppingToken);
            
            if (finishedAuctions.Count == 0 ) return;

            _logger.LogInformation("===> found {0} auctions that have completed", finishedAuctions.Count);

            using var scope = _services.CreateScope();

            //**as background service is singleton, but publishEnpoint is scope, so
            //**it is not allow to inject something that got different lifetime
            
            var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            foreach (var auction in finishedAuctions)
            {
                auction.Finished = true;
                await auction.SaveAsync(null, stoppingToken);
                var winningBid = await DB.Find<Bid>()
                        .Match(x=> x.AuctionId == auction.ID)
                        .Match(x=> x.BidStatus == BidStatus.Accepted)
                        .Sort(x=> x.Descending(s=> s.Amount))
                        .ExecuteFirstAsync(stoppingToken);

                await endpoint.Publish(new AuctionFinished
                {
                    ItemSold = winningBid != null,
                    AuctionId = auction.ID,
                    Winner = winningBid?.Bidder,
                    Amount = winningBid?.Amount,
                    Seller = auction.Seller
                }, stoppingToken);
            }

        }
    }
}