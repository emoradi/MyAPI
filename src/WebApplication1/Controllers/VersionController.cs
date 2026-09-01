using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VersionController : ControllerBase
    {
        

        [HttpGet(Name = "GetVersion")]
        public string Get()
        {
            return "34";
        }
    }
}
