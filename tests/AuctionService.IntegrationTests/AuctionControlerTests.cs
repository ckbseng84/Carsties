using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Util;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests
{
    [Collection("Shared collection")]
    public class AuctionControlerTests : IAsyncLifetime
    {
        private readonly CustomWebAppFactory _factory;
        private readonly HttpClient _httpClient;
        private const string GT_ID = "afbee524-5972-4075-8800-7d1f9d7b0a0c";
        public AuctionControlerTests(CustomWebAppFactory factory)
        {
            _factory = factory;
            _httpClient = factory.CreateClient();

        }
        [Fact]
        public async Task GetAuctions_ShouldReturn3Auctions()
        {
            // Given

            // When
            var response = await _httpClient.GetFromJsonAsync<List<AuctionDto>>("api/auctions");

            // Then
            Assert.Equal(3, response.Count);
        }

        [Fact]
        public async Task GetAuctionById_WithValidId_ShouldReturnAuction()
        {
            // Given

            // When
            var response = await _httpClient.GetFromJsonAsync<AuctionDto>($"api/auctions/{GT_ID}");

            // Then
            Assert.Equal("GT", response.Model);
        }

        [Fact]
        public async Task GetAuctionById_WithInvalidId_ShouldReturn404()
        {
            // Given

            // When
            var response = await _httpClient.GetAsync($"api/auctions/{Guid.NewGuid()}");

            // Then
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetAuctionById_WithInvalidGuid_ShouldReturn400()
        {
            // Given

            // When
            var response = await _httpClient.GetAsync($"api/auctions/notaguid");

            // Then
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateAuction_WithNoAuth_ShouldReturn401()
        {
            // Given
            var auction = new CreateAuctionDto { Make = "test" };
            // When
            var response = await _httpClient.PostAsJsonAsync($"api/auctions", auction);

            // Then
            //response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateAuction_WithAuth_ShouldReturn201()
        {
            // Given
            var auction = GetAuctionForCreate();
            _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

            // When
            var response = await _httpClient.PostAsJsonAsync($"api/auctions", auction);

            // Then
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var createdAuction = await response.Content.ReadFromJsonAsync<AuctionDto>();
            Assert.Equal("bob", createdAuction.Seller);
        }

        [Fact]
        public async Task CreateAuction_WithInvalidCreateAuctionDto_ShouldReturn400()
        {
            // arrange
            var auction = GetAuctionForCreate();
            auction.Make = null;
            _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

            // act
            var response = await _httpClient.PostAsJsonAsync($"api/auctions", auction);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        

        [Fact]
        public async Task UpdateAuction_WithValidUpdateDtoAndUser_ShouldReturn200()
        {
            // arrange
            var updateAuctionDto = new UpdateAuctionDto
            {
                Model = "updated"

            };
            _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

            // act
            var response = await _httpClient.PutAsJsonAsync($"api/auctions/{GT_ID}", updateAuctionDto);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task UpdateAuction_WithValidUpdateDtoAndInvalidUser_ShouldReturn403()
        {
            // arrange
            var updateAuctionDto = new UpdateAuctionDto{Make = "updated"};
            _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("testing"));

            // act
            var response = await _httpClient.PutAsJsonAsync($"api/auctions/{GT_ID}", updateAuctionDto);

            // assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }


        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
            DBHelper.ReinitDbForTests(db);
            return Task.CompletedTask;
        }
        private static CreateAuctionDto GetAuctionForCreate()
        {
            return new CreateAuctionDto
            {
                Make = "test",
                Model = "testmodel",
                ImageUrl = "test",
                Mileage = 100,
                Year = 2022,
                ReservePrice = 1000,
                Color = "Test"
            };
        }

    }
}