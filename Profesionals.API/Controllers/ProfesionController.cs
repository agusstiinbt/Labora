using Microsoft.AspNetCore.Mvc;
using Professionals.Infrastructure;

namespace Profesionals.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfesionController : ControllerBase
    {
        [MinimumAgeAuthorize(21)]
        [HttpGet(Name = "RegisterAsProfessional")]
        public ActionResult Get()
        {
            return Ok("Register as professional endpoint is working.");
        }
    }
}
