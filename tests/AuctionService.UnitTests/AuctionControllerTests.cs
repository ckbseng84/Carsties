using AuctionService.Controllers;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.RequestHelpers;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuctionService.UnitTests;

public class AuctionControllerTests
{
    private readonly Mock<IAuctionRepository> _auctionRepo;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly Fixture _fixture;
    private readonly AuctionsController _controller;
    private readonly IMapper _mapper;
    public AuctionControllerTests()
    {
        _fixture = new Fixture();;
        _auctionRepo = new Mock<IAuctionRepository>();
        _publishEndpoint = new Mock<IPublishEndpoint>();
        var mockMapper = new MapperConfiguration(mc=> {
            mc.AddMaps(typeof(MappingProfile).Assembly);
        }).CreateMapper().ConfigurationProvider;
        _mapper = new Mapper(mockMapper);
        _controller = new AuctionsController(_auctionRepo.Object, _mapper,_publishEndpoint.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext{User = Helpers.GetClaimsPrincipal()}
            }
        };
    }
    [Fact]
    public async Task GetAllAuctions_WithNoParams_Returns10Auctions()
    {
        // Given
        var auctions = _fixture.CreateMany<AuctionDto>(10).ToList();
        _auctionRepo.Setup(repo => repo.GetAutionsAsync(null)).ReturnsAsync(auctions);
        // When
        var result = await _controller.GetAllAuctions(null);
        // Then
        Assert.Equal(10, result.Value.Count);
        Assert.IsType<ActionResult<List<AuctionDto>>>(result);
    }
    [Fact]
    public async Task GetAuctionById_WithValidGuid_ReturnAuction()
    {
        // Given
        var auction = _fixture.Create<AuctionDto>();
        _auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
        // When
        var result = await _controller.GetAuctionById(auction.Id);
        // Then
        Assert.Equal(auction.Make, result.Value.Make);
        Assert.IsType<ActionResult<AuctionDto>>(result);
    }
    [Fact]
    public async Task GetAuctionById_WithInvalidGuid_ReturnNotFound()
    {
        // Given
        _auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);
        // When
        var result = await _controller.GetAuctionById(Guid.NewGuid());
        // Then
        Assert.IsType<NotFoundResult>(result.Result);
    }
    
    [Fact]
    public async Task CreateAuction_WithValidCreateAuctionDto_ReturnCreatedAtAction()
    {
        // Given
        var auction = _fixture.Create<CreateAuctionDto>();
        _auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        // When
        var result = await _controller.CreateAuction(auction);
        var createdResult = result.Result as CreatedAtActionResult;

        // Then
        Assert.NotNull(createdResult);
        Assert.Equal("GetAuctionById", createdResult.ActionName);
        Assert.IsType<AuctionDto>(createdResult.Value);
    }
    [Fact]
    public async Task CreateAuction_FailedSave_Returns400BadRequest()
    {
        // Given
        var auction = _fixture.Create<CreateAuctionDto>();
        _auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);

        // When
        var result = await _controller.CreateAuction(auction);

        // Then
        Assert.NotNull(result);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateAuction_WithUpdateAuctionDto_ReturnsOkResponse()
    {
        // Given
        var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();
        var auction = _fixture.Build<Auction>().Without(x=> x.Item).Create();
        var item = _fixture.Build<Item>().Without(x=> x.Auction).Create();
        auction.Item = item;
        auction.Seller ="test";
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>()))
            .ReturnsAsync(auction);
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        // When
        var result = await _controller.UpdateAuction(It.IsAny<Guid>(), updateAuctionDto);

        // Then
        Assert.NotNull(result);
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidUser_Returns403Forbid()
    {
        // Given
        var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();
        var auction = _fixture.Build<Auction>().Without(x=> x.Item).Create();

        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>()))
            .ReturnsAsync(auction);
     
        // When
        var result = await _controller.UpdateAuction(It.IsAny<Guid>(), updateAuctionDto);

        // Then
        Assert.NotNull(result);
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidGuid_ReturnsNotFound()
    {
        // Given
        var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();

        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>()))
            .ReturnsAsync(value:null);
     
        // When
        var result = await _controller.UpdateAuction(It.IsAny<Guid>(), updateAuctionDto);

        // Then
        Assert.NotNull(result);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithValidUser_ReturnsOkResponse()
    { 
        // Given
        var auction = _fixture.Build<Auction>().Without(x=> x.Item).Create();
        auction.Seller = "test";
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(repo=> repo.SaveChangesAsync()).ReturnsAsync(true);
        // When
        var result = await _controller.DeleteAuction(It.IsAny<Guid>());
        // Then
        Assert.NotNull(result);
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidGuid_Returns404Response()
    {
        // Given
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>()))
            .ReturnsAsync(It.IsAny<Auction>());
        // When
        var result = await _controller.DeleteAuction(It.IsAny<Guid>());
        // Then
        Assert.NotNull(result);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidUser_Returns403Response()
    {
        // Given
         var auction = _fixture.Build<Auction>().Without(x=> x.Item).Create();
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(auction.Id)).ReturnsAsync(auction);
        // When
        var result = await _controller.DeleteAuction(auction.Id);
        // Then
        Assert.NotNull(result);
        Assert.IsType<ForbidResult>(result);
    }
}
