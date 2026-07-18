using Microsoft.AspNetCore.Mvc;
using PhotoProcessor.DTO.enums;
using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.ServiceLogic;

namespace PhotoProcessor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProgressController(IProgressLogic progressLogic) : Controller
    {
        [HttpGet(nameof(GetProgress))]
        public async Task<ActionResult<List<JobTypeProgress>>> GetProgress(CancellationToken cancellationToken)
        {
            return Ok(await progressLogic.GetProgress(cancellationToken));
        }

        [HttpPost(nameof(Backfill))]
        public async Task<ActionResult> Backfill([FromQuery] JobTypes jobType, CancellationToken cancellationToken)
        {
            int enqueued = await progressLogic.Backfill(jobType, cancellationToken);
            return Ok(new { jobType, enqueued });
        }
    }
}
