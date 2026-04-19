using Microsoft.AspNetCore.Mvc;
using PlantMonitoringAPI.Data;
using PlantMonitoringAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace PlantMonitoringAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GroupController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/groups
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupDTO>>> GetGroups()
        {
            var groups = await _context.Groups
                .Select(p => new GroupDTO
                {
                    Id = p.Id,
                    Name = p.Name
                })
                .ToListAsync();

            return Ok(groups);
        }
    }
}
