using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RentalManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantsController : ControllerBase
    {
        private readonly RentalManagementContext _context;

        public TenantsController(RentalManagementContext context)
        {
            _context = context;
        }

        // GET: api/Tenants
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tenant>>> GetTenants()
        {
            return await _context.Tenants.ToListAsync();
        }

        // GET: api/Tenants/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tenant>> GetTenant(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null)
            {
                return NotFound();
            }

            return tenant;
        }

        // POST: api/Tenants
        [HttpPost]
        public async Task<ActionResult<Tenant>> CreateTenant(Tenant tenant)
        {
            tenant.Id = 0;
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTenant), new { id = tenant.Id }, tenant);
        }

        // PUT: api/Tenants/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTenant(int id, Tenant tenant)
        {
            if (id != tenant.Id)
            {
                return BadRequest();
            }

            _context.Entry(tenant).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TenantExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Tenants/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTenant(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null)
            {
                return NotFound();
            }

            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TenantExists(int id)
        {
            return _context.Tenants.Any(e => e.Id == id);
        }
    }
}
