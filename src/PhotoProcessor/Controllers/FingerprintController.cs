using Microsoft.AspNetCore.Mvc;
using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.ServiceLogic;

namespace PhotoProcessor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FingerprintController(
        IFingerprintSubmissionLogic fingerprintSubmissionLogic,
        IFingerprintSearchLogic fingerprintSearchLogic
        ) : Controller
    {
        [HttpPost(nameof(Submit))]
        public async Task<ActionResult> Submit([FromBody] SubmitFingerprintsRequest request, CancellationToken cancellationToken)
        {
            await fingerprintSubmissionLogic.Submit(request, cancellationToken);
            return Ok();
        }

        [HttpGet(nameof(SearchByFingerprint))]
        public async Task<ActionResult<List<FingerprintMatch>>> SearchByFingerprint([FromQuery] Guid fingerprintId, [FromQuery] int count, CancellationToken cancellationToken)
        {
            List<FingerprintMatch> matches = await fingerprintSearchLogic.SearchByFingerprint(fingerprintId, count <= 0 ? 10 : count, cancellationToken);
            return Ok(matches);
        }
    }
}
