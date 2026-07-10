using Microsoft.AspNetCore.Mvc;
using PhotoProcessor.Logic.ServiceLogic;

namespace PhotoProcessor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotoController(IPhotoLogic photoLogic) : Controller
    {
        
        [HttpGet(nameof(GetDownloadUrl))]
        public async Task<ActionResult> GetDownloadUrl(Guid mediaId, CancellationToken cancellationToken)
        {
            Uri url = await photoLogic.GetDownloadUrl(mediaId, cancellationToken);
            return Ok(new { url = url.ToString() });
        }

        [HttpPost(nameof(Upload))]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
                return BadRequest("No file provided.");

            await using Stream content = file.OpenReadStream();
            Guid mediaId = await photoLogic.Upload(file.FileName, content, cancellationToken);
            return Ok(new { mediaId });
        }

        [HttpGet(nameof(Download))]
        public async Task<ActionResult> Download([FromQuery] Guid mediaId, CancellationToken cancellationToken)
        {
            (Stream stream, string contentType) = await photoLogic.GetImage(mediaId, cancellationToken);
            return File(stream, contentType);
        }

        [HttpGet(nameof(Thumbnail))]
        public async Task<ActionResult> Thumbnail([FromQuery] Guid mediaId, [FromQuery] int width, CancellationToken cancellationToken)
        {
            Stream stream = await photoLogic.GetThumbnail(mediaId, width <= 0 ? 320 : width, cancellationToken);
            return File(stream, "image/jpeg");
        }

        [HttpGet(nameof(GetFaceThumbnail))]
        public async Task<ActionResult> GetFaceThumbnail([FromQuery] Guid fingerprintId, [FromQuery] int width, CancellationToken cancellationToken)
        {
            Stream stream = await photoLogic.GetFaceThumbnail(fingerprintId, width <= 0 ? 160 : width, cancellationToken);
            return File(stream, "image/jpeg");
        }
    }
}
