using Microsoft.AspNetCore.Mvc;
using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.ServiceLogic;

namespace PhotoProcessor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlowerController(
        IFlowerIdentificationSubmissionLogic flowerIdentificationSubmissionLogic,
        IFlowerIdentificationQueryLogic flowerIdentificationQueryLogic
        ) : Controller
    {
        [HttpPost(nameof(Submit))]
        public async Task<ActionResult> Submit([FromBody] SubmitFlowerIdentificationRequest request, CancellationToken cancellationToken)
        {
            await flowerIdentificationSubmissionLogic.Submit(request, cancellationToken);
            return Ok();
        }

        [HttpGet(nameof(GetForMedia))]
        public async Task<ActionResult<FlowerIdentificationSummary>> GetForMedia([FromQuery] Guid mediaId, CancellationToken cancellationToken)
        {
            FlowerIdentificationSummary? summary = await flowerIdentificationQueryLogic.GetForMedia(mediaId, cancellationToken);
            return summary is null ? NotFound() : Ok(summary);
        }
    }
}
