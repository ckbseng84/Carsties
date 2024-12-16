using AuctionService.Data;
using Grpc.Core;

namespace AuctionService
{
    // ensure install Grpc.aspnetcore package, 
    // otherwise, GrpcAuction would not be generated automatically
    public class GrpcAuctionService : GrpcAuction.GrpcAuctionBase
    {
        private readonly AuctionDbContext _dbContext;

        public GrpcAuctionService(AuctionDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public override async Task<GrpcAuctionResponse> GetAuction(GetAuctionRequest request, 
        ServerCallContext context)
        {
            Console.WriteLine("==> receive grpc getAuction");
            var auction = await _dbContext.Auctions.FindAsync(Guid.Parse(request.Id)) 
                ?? throw new RpcException(new Status(StatusCode.NotFound,"Not Found"));

            var response = new GrpcAuctionResponse
            {
                Auction = new GrpcAuctionModel
                {
                    AuctionEnd = auction.AuctionEnd.ToString(),
                    Id = auction.Id.ToString(),
                    ReservicePrice = auction.ReservePrice,
                    Seller = auction.Seller,
                }
            };
            return response;
        }
    }
}