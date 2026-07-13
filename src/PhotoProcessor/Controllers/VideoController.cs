using Microsoft.AspNetCore.Mvc;
using PhotoProcessor.DTO.enums;
using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.ServiceLogic;

namespace PhotoProcessor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoController(IMediaIngestLogic mediaIngestLogic, IVideoLogic videoLogic) : Controller
    {
        [HttpPost(nameof(Upload))]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
                return BadRequest("No file provided.");

            await using Stream content = file.OpenReadStream();
            (Guid mediaId, bool duplicate) = await mediaIngestLogic.Store(file.FileName, content, MediaItemType.Video, cancellationToken);
            return Ok(new { mediaId, duplicate });
        }
         
        [HttpPost(nameof(Submit))]
        public async Task<ActionResult> Submit([FromBody] SubmitVideoRequest request, CancellationToken cancellationToken)
        {
            await videoLogic.Submit(request, cancellationToken);
            return Ok();
        }

        [HttpGet(nameof(GetRenditions))]
        public async Task<ActionResult<List<VideoRenditionSummary>>> GetRenditions([FromQuery] Guid mediaId, CancellationToken cancellationToken)
        {
            return Ok(await videoLogic.GetRenditions(mediaId, cancellationToken));
        }

        [HttpGet("Stream/{mediaId:guid}/{**assetPath}")]
        public async Task<ActionResult> Stream(Guid mediaId, string assetPath, CancellationToken cancellationToken)
        {
            (Stream stream, string contentType) = await videoLogic.GetDerivedAsset(mediaId, assetPath, cancellationToken);
            return File(stream, contentType);
        }
    }
}
