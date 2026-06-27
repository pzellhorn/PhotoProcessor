using Microsoft.AspNetCore.Mvc;
using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.ServiceLogic;

namespace PhotoProcessor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FingerprintController(IFingerprintSubmissionLogic fingerprintSubmissionLogic) : Controller
    {
        [HttpPost(nameof(Submit))]
        public async Task<ActionResult> Submit([FromBody] SubmitFingerprintsRequest request, CancellationToken cancellationToken)
        {
            await fingerprintSubmissionLogic.Submit(request, cancellationToken);
            return Ok();
        }
    }
}
