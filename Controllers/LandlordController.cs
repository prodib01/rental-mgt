using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;

namespace RentalManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LandlordsController : ControllerBase
    {
        private readonly RentalManagementContext _context;

        public LandlordsController(RentalManagementContext context)
        {
            _context = context;
        }

        // GET: api/Landlords
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Landlord>>> GetLandlords()
        {
            return await _context.Landlords.ToListAsync();
        }

        // GET: api/Landlords/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Landlord>> GetLandlord(int id)
        {
            var landlord = await _context.Landlords.FindAsync(id);
            if (landlord == null)
            {
                return NotFound();
            }

            return landlord;
        }

        // POST: api/Landlords
        [HttpPost]
        public async Task<ActionResult<Landlord>> CreateLandlord(Landlord landlord)
        {
            landlord.Id = 0;
            _context.Landlords.Add(landlord);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLandlord), new { id = landlord.Id }, landlord);
        }

        // PUT: api/Landlords/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLandlord(int id, Landlord landlord)
        {
            if (id != landlord.Id)
            {
                return BadRequest();
            }

            _context.Entry(landlord).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LandlordExists(id))
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

        // DELETE: api/Landlords/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLandlord(int id)
        {
            var landlord = await _context.Landlords.FindAsync(id);
            if (landlord == null)
            {
                return NotFound();
            }

            _context.Landlords.Remove(landlord);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LandlordExists(int id)
        {
            return _context.Landlords.Any(e => e.Id == id);
        }
    }
}
