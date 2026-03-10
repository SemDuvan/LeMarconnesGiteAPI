using LeMarconnesGiteAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeMarconnesGiteAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuditLogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuditLogsController(AppDbContext context)
        {
            _context = context;
        }

        // FR-09: GET /auditlogs — alle audit logs (read-only)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _context.AuditLogs
                .OrderByDescending(l => l.ChangedAt)
                .ToListAsync();

            return Ok(logs);
        }

        // FR-09: GET /auditlogs/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var log = await _context.AuditLogs.FindAsync(id);
            if (log == null)
                return NotFound($"Audit log met id {id} niet gevonden.");

            return Ok(log);
        }
    }
}
