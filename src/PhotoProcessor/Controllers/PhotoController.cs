using Microsoft.AspNetCore.Mvc;
using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.ServiceLogic;

namespace PhotoProcessor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotoController(IPhotoLogic photoLogic) : Controller
    {
        [HttpPost(nameof(CreateUpload))]
        public async Task<ActionResult<CreateUploadResponse>> CreateUpload([FromBody] CreateUploadRequest request, CancellationToken cancellationToken)
        {
            (Guid mediaId, Uri uploadUrl) = await photoLogic.CreateUpload(request.FileName, cancellationToken);
            return Ok(new CreateUploadResponse(mediaId, uploadUrl.ToString()));
        }

        [HttpGet(nameof(GetDownloadUrl))]
        public async Task<ActionResult> GetDownloadUrl(Guid mediaId, CancellationToken cancellationToken)
        {
            Uri url = await photoLogic.GetDownloadUrl(mediaId, cancellationToken);
            return Ok(new { url = url.ToString() });
        }
    }
}
