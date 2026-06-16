using System.IO;
using Microsoft.AspNetCore.Mvc;
using PhotoProcessor.DTO.ResponseDTOs.EntityResponses;
using PhotoProcessor.Logic.ServiceLogic;
using pzellhorn.Core.State.Storage;

namespace PhotoProcessor.API.Controllers
{  
    [ApiController]
    [Route("api/[controller]")]
    public class PhotoController(IPhotoLogic photoLogic) : Controller
    { 
        [HttpPost(nameof(IngestPhoto))]
        public async Task<ActionResult> IngestPhoto(IFormFile file, CancellationToken cancellationToken)
        {
            await using Stream stream = file.OpenReadStream();

            await photoLogic.IngestPhoto(file.FileName, stream, cancellationToken);

            return Ok();  
        }

    } 
}
