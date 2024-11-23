using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController : ControllerBase// a base class for mvc controller without view
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;

        public AuctionsController(IAuctionRepository auctionRepository, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _auctionRepository = auctionRepository;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
        }
        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>>GetAllAuctions(string date)// string as date, mongodb use this for datetime
        {
            return await _auctionRepository.GetAutionsAsync(date);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await _auctionRepository.GetAuctionByIdAsync(id);

            if (auction == null) return NotFound();
            
            return auction;
        }
        
        // create auction
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            //map to auction entity
            var auction = _mapper.Map<Auction>(auctionDto);
            //todo: add current user as user
            //set seller name
            auction.Seller= User.Identity.Name;
            //add to db, but not yet commit
            _auctionRepository.AddAuction(auction);
            //map to AuctionDto
            var newAuction = _mapper.Map<AuctionDto>(auction); 

            //publish the new AuctionDto
            await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));
            
            // if success from mass transit, commit the change
            var result = await _auctionRepository.SaveChangesAsync();
            if (!result) return BadRequest("Could not save changes to DB");

            //call GetAuctionById to get auction object
            return CreatedAtAction(nameof(GetAuctionById), 
                new {auction.Id}, newAuction);
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await _auctionRepository.GetAuctionEntityById(id);
            
            if (auction == null) return NotFound();
            
            if (auction.Seller != User.Identity.Name) return Forbid();
            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
            //publish to mass transit for this update
            await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

            var result = await _auctionRepository.SaveChangesAsync();

            if (result) return Ok();

            return BadRequest("unable to save");
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _auctionRepository.GetAuctionEntityById(id);
            if (auction == null) return NotFound();

            if (auction.Seller != User.Identity.Name) return Forbid();

            _auctionRepository.RemoveAuction(auction);
            //publish to mass transit for remove this auction
            await _publishEndpoint.Publish<AuctionDeleted>(new {Id = auction.Id.ToString()});
            
            var result = await _auctionRepository.SaveChangesAsync();

            if(!result) return BadRequest("Could not update db");
            return Ok();

        } 
    }
}