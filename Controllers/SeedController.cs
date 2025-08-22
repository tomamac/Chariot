using Chariot.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chariot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController(ISeedService seedService) : ControllerBase
    {
        [HttpGet("admin-seed")]
        public async Task<ActionResult> AdminSeed()
        {
            var res = await seedService.AdminSeedAsync();
            if (res is null)
            {
                return BadRequest("Admin is seeded or duplicate username");
            }
            return Ok("Seeded");
        }
    }
}
