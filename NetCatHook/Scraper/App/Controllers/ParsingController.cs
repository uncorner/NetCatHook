using Microsoft.AspNetCore.Mvc;

namespace NetCatHook.Scraper.App.Controllers;

[ApiController]
[Route("parsing")]
public class ParsingController : ControllerBase
{

    [HttpGet("sample")]
    public async Task<IActionResult> Sample()
    {
        return await Task.FromResult(Ok("Sample API response"));
    }


}
