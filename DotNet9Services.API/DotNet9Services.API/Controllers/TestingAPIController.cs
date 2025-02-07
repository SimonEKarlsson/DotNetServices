using Microsoft.AspNetCore.Mvc;

namespace DotNet9Services.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestingAPIController : ControllerBase
    {


        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello");
        }

    }
}
