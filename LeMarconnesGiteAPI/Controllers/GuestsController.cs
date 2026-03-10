using LeMarconnesGiteAPI.Data;
using LeMarconnesGiteAPI.DTOs;
using LeMarconnesGiteAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeMarconnesGiteAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GuestsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GuestsController(AppDbContext context)
        {
            _context = context;
        }

        // GET /guests
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var guests = await _context.Guests.ToListAsync();
            return Ok(guests);
        }

        // GET /guests/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var guest = await _context.Guests.FindAsync(id);
            if (guest == null)
                return NotFound($"Gast met id {id} niet gevonden.");

            return Ok(guest);
        }

        // POST /guests
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGuestDto dto)
        {
            var guest = new Guest
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone
            };

            _context.Guests.Add(guest);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = guest.Id }, guest);
        }

        // PUT /guests/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateGuestDto dto)
        {
            var guest = await _context.Guests.FindAsync(id);
            if (guest == null)
                return NotFound($"Gast met id {id} niet gevonden.");

            if (dto.Name != null) guest.Name = dto.Name;
            if (dto.Email != null) guest.Email = dto.Email;
            if (dto.Phone != null) guest.Phone = dto.Phone;

            await _context.SaveChangesAsync();
            return Ok(guest);
        }

        // DELETE /guests/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var guest = await _context.Guests.FindAsync(id);
            if (guest == null)
                return NotFound($"Gast met id {id} niet gevonden.");

            _context.Guests.Remove(guest);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
