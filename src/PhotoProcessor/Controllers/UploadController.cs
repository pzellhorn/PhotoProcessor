using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.ServiceLogic;

namespace PhotoProcessor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController(IResumableUploadLogic resumableUploadLogic) : Controller
    {
        [HttpPost(nameof(Create))]
        public async Task<ActionResult<CreateUploadResponse>> Create([FromBody] CreateUploadRequest request, CancellationToken cancellationToken)
        {
            return Ok(await resumableUploadLogic.Create(request, cancellationToken));
        }

        [HttpPut(nameof(Chunk))]
        [DisableRequestSizeLimit]
        public async Task<ActionResult<UploadStatusResponse>> Chunk([FromQuery] Guid uploadId, CancellationToken cancellationToken)
        {
            if (!TryGetRangeStart(Request.Headers.ContentRange, out long offset))
                return BadRequest("A Content-Range header of the form 'bytes {start}-{end}/{total}' is required.");

            UploadStatusResponse status = await resumableUploadLogic.AppendChunk(uploadId, offset, Request.Body, cancellationToken);
            return Ok(status);
        }

        [HttpGet(nameof(Status))]
        public async Task<ActionResult<UploadStatusResponse>> Status([FromQuery] Guid uploadId, CancellationToken cancellationToken)
        {
            return Ok(await resumableUploadLogic.GetStatus(uploadId, cancellationToken));
        }

        [HttpDelete(nameof(Abort))]
        public async Task<ActionResult> Abort([FromQuery] Guid uploadId, CancellationToken cancellationToken)
        {
            await resumableUploadLogic.Abort(uploadId, cancellationToken);
            return NoContent();
        }

        private static bool TryGetRangeStart(string? contentRange, out long start)
        {
            start = 0;
            if (string.IsNullOrWhiteSpace(contentRange))
                return false;

            string value = contentRange.Trim();
            if (value.StartsWith("bytes ", StringComparison.OrdinalIgnoreCase))
                value = value["bytes ".Length..];

            int dash = value.IndexOf('-');
            if (dash <= 0)
                return false;

            return long.TryParse(value[..dash], NumberStyles.None, CultureInfo.InvariantCulture, out start);
        }
    }
}
