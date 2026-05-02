using IR_SearchEngine.Core.DTOs;
using IR_SearchEngine.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IRProject.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _service;
        public SearchController(ISearchService service) => _service = service;

        [HttpPost("query")]
        public IActionResult Search([FromBody] SearchRequestDto dto) => Ok(_service.Search(dto));
    }
}