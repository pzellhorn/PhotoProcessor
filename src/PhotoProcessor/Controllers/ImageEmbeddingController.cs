using Microsoft.AspNetCore.Mvc;
using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.ServiceLogic;

namespace PhotoProcessor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageEmbeddingController(IImageEmbeddingSubmissionLogic imageEmbeddingSubmissionLogic) : Controller
    {
        [HttpPost(nameof(Submit))]
        public async Task<ActionResult> Submit([FromBody] SubmitImageEmbeddingRequest request, CancellationToken cancellationToken)
        {
            await imageEmbeddingSubmissionLogic.Submit(request, cancellationToken);
            return Ok();
        }
    }
}
