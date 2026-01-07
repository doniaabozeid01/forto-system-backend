using Forto.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forto.Api.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    [Route("api/services")]
    public class ServicesController : BaseApiController
    {
        [HttpPost]
        public IActionResult Create(CreateServiceRequest request)
        {
            // هنا بعدين هننادي Application service ونحفظ في DB
            var created = new { Id = 1, request.Name, request.Price, request.DurationMinutes };
            return CreatedResponse(created);
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var data = new { Id = id, Name = "Wash", Price = 100, DurationMinutes = 30 };
            return OkResponse(data);
        }
    }
}
