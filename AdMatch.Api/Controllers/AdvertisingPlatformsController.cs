using AdMatch.MicroserviceInterfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdMatch.Api.Controllers;

[ApiController]
[Route("api/advertising")]
public class AdvertisingPlatformsController(IAdvertisingService placementService) : AdvertisingPlatformsControllerBase
{
    [HttpPost("load")]
    public override async Task<IActionResult> LoadPlatforms(IFormFile file)
    {
        try
        {
            await placementService.LoadPlatformsFromFileAsync(file);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("search")]
    public override async Task<ActionResult<IEnumerable<string>>> SearchPlatforms(string location)
    {
        try
        {
            var result = await placementService.SearchPlatformsAsync(location);
            return Ok(result);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Internal server error");
        }
    }
}