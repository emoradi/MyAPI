using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/version")]
public class VersionController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            version = Environment.GetEnvironmentVariable("APP_VERSION"),
            pod = Environment.GetEnvironmentVariable("HOSTNAME")
        });
    }
}