using PhotoProcessor.DTO.DTOAdapters.Interfaces;
using PhotoProcessor.DTO.RequestDTOs.EntityRequests;
using PhotoProcessor.DTO.ResponseDTOs.EntityResponses;
using Microsoft.AspNetCore.Mvc;
using pzellhorn.Core;

namespace PhotoProcessor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagItemController(ITagItemDtoAdapter logic) : BaseController<TagItemRequest, TagItemResponse>(logic)
    {
    }
}
