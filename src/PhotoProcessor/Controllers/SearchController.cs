using Microsoft.AspNetCore.Mvc;
using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.ServiceLogic;

namespace PhotoProcessor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController(ISearchLogic searchLogic) : Controller
    {
        [HttpGet(nameof(SearchByText))]
        public async Task<ActionResult<List<MediaSearchResult>>> SearchByText([FromQuery] string text, [FromQuery] int count, CancellationToken cancellationToken)
        {
            List<MediaSearchResult> results = await searchLogic.SearchByText(text, count <= 0 ? 20 : count, cancellationToken);
            return Ok(results);
        }
    }
}
