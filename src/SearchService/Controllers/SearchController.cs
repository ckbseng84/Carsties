using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.RequestHelpers;

namespace SearchService.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<Item>>> SearchItem([FromQuery]SearchParams searchParams )
        {
            // searching begin
            var query = DB.PagedSearch<Item,Item>();

            //simple search term
            if (!string.IsNullOrEmpty(searchParams.SearchTerm))
            {
                query.Match(Search.Full, searchParams.SearchTerm) //todo: to be studied
                    .SortByTextScore();// todo: detail of this function
            }

            // ordering mechanism
            query = searchParams.OrderBy switch
            {
                "make" => query.Sort(x=> x.Ascending(a=> a.Make))
                    .Sort(x=> x.Ascending(a => a.Model)),
                "new" => query.Sort(x=> x.Descending(a=> a.CreatedAt)),
                _ => query.Sort(x=> x.Ascending(a=> a.AuctionEnd))// default sorting
            };

            //filtering auctionEnd mechanism
            query = searchParams.FilterBy switch
            {
                "finished" => query.Match(x=> x.AuctionEnd < DateTime.UtcNow),
                "endingSoon" => query.Match(x=> x.AuctionEnd < DateTime.UtcNow.AddHours(6) 
                    && x.AuctionEnd > DateTime.UtcNow),
                _ => query.Match(x=> x.AuctionEnd > DateTime.UtcNow)
            };

            // filter by seller
            if (!string.IsNullOrEmpty(searchParams.Seller))
            {
                query.Match(x=> x.Seller == searchParams.Seller);
            }
            
            //filter by Winner
            if (!string.IsNullOrEmpty(searchParams.Winner))
            {
                query.Match(x=> x.Winner == searchParams.Winner);
            }

            //Set pagination
            query.PageNumber(searchParams.PageNumber);
            query.PageSize(searchParams.PageSize);

            var result = await query.ExecuteAsync();

            return Ok(new 
            {
                results = result.Results, 
                pageCount = result.PageCount,   //set pageCount
                totalCount = result.TotalCount, //set totalCount
            });
        }
    }
}