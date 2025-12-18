using DAL.Services.Logs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class LogsController : ControllerBase
    {
        private readonly ILogService _service;

        public LogsController(ILogService service)
        {
            _service = service;
        }

        [HttpGet("get/{n}")]
        public IActionResult GetLatest(int n)
            => Ok(_service.GetLatest(n));

        [HttpGet("count")]
        public IActionResult Count()
            => Ok(new { count = _service.Count(null, null) });
    }
}
