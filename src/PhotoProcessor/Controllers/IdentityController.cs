using Microsoft.AspNetCore.Mvc;
using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.ServiceLogic;

namespace PhotoProcessor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IdentityController(IIdentityLogic identityLogic) : Controller
    {
        [HttpGet(nameof(ListIdentities))]
        public async Task<ActionResult<List<IdentitySummary>>> ListIdentities(CancellationToken cancellationToken)
        {
            List<IdentitySummary> identities = await identityLogic.ListIdentities(cancellationToken);
            return Ok(identities);
        }

        [HttpGet(nameof(GetFacesForTag))]
        public async Task<ActionResult<List<FingerprintSummary>>> GetFacesForTag([FromQuery] Guid tagId, CancellationToken cancellationToken)
        {
            List<FingerprintSummary> faces = await identityLogic.GetFacesForTag(tagId, cancellationToken);
            return Ok(faces);
        }

        [HttpPost(nameof(Merge))]
        public async Task<ActionResult> Merge([FromBody] MergeIdentitiesRequest request, CancellationToken cancellationToken)
        {
            await identityLogic.Merge(request.SourceTagId, request.TargetTagId, cancellationToken);
            return Ok();
        }
    }
}
