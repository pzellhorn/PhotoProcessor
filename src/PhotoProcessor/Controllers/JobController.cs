using PhotoProcessor.DTO.DTOAdapters.Interfaces;
using PhotoProcessor.DTO.RequestDTOs.EntityRequests;
using PhotoProcessor.DTO.ResponseDTOs.EntityResponses;
using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.EntityLogic;
using Microsoft.AspNetCore.Mvc;
using pzellhorn.Core;

namespace PhotoProcessor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobController(IJobDtoAdapter logic, JobLogic jobLogic) : BaseController<JobRequest, JobResponse>(logic)
    {
        [HttpPost(nameof(MarkRunning))]
        public async Task<ActionResult> MarkRunning([FromBody] MarkJobRunningRequest request, CancellationToken cancellationToken)
        {
            await jobLogic.MarkRunning(request.JobId, cancellationToken);
            return Ok();
        }

        [HttpPost(nameof(MarkFailed))]
        public async Task<ActionResult> MarkFailed([FromBody] MarkJobFailedRequest request, CancellationToken cancellationToken)
        {
            await jobLogic.MarkFailed(request.JobId, request.Error, cancellationToken);
            return Ok();
        }
    }
}
