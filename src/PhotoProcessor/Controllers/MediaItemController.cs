using PhotoProcessor.DTO.DTOAdapters.Interfaces;
using PhotoProcessor.DTO.RequestDTOs.EntityRequests;
using PhotoProcessor.DTO.ResponseDTOs.EntityResponses;
using Microsoft.AspNetCore.Mvc;
using pzellhorn.Core;

namespace PhotoProcessor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaItemController(IMediaItemDtoAdapter logic) : BaseController<MediaItemRequest, MediaItemResponse>(logic)
    {
        [HttpPost(nameof(IngestPhoto))]
        public async Task<ActionResult<MediaItemResponse>> IngestPhoto([FromBody] MediaItemRequest request, CancellationToken cancellationToken)
        {
            return Ok(await logic.IngestPhoto(request, cancellationToken));
        } 
    }
}
