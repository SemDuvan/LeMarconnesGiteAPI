using LeMarconnesGiteAPI.Data;
using LeMarconnesGiteAPI.DTOs;
using LeMarconnesGiteAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeMarconnesGiteAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GitesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GitesController(AppDbContext context)
        {
            _context = context;
        }

        // FR-01: GET /gites?personen=4 — filter op MaxOccupancy
        // FR-02: Beschikbaarheidsstatus inbegrepen
        // FR-03: Faciliteiten worden meegestuurd
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? personen)
        {
            var query = _context.Gites
                .Include(g => g.GiteFacilities)
                    .ThenInclude(gf => gf.Facility)
                .AsQueryable();

            if (personen.HasValue)
                query = query.Where(g => g.MaxOccupancy >= personen.Value);

            var gites = await query.Select(g => new
            {
                g.Id,
                g.Name,
                g.Description,
                g.MaxOccupancy,
                g.PricePerNight,
                g.IsAvailable,
                Facilities = g.GiteFacilities.Select(gf => gf.Facility!.Name)
            }).ToListAsync();

            return Ok(gites);
        }

        // GET /gites/{id} — één gîte met faciliteiten
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var gite = await _context.Gites
                .Include(g => g.GiteFacilities)
                    .ThenInclude(gf => gf.Facility)
                .Where(g => g.Id == id)
                .Select(g => new
                {
                    g.Id,
                    g.Name,
                    g.Description,
                    g.MaxOccupancy,
                    g.PricePerNight,
                    g.IsAvailable,
                    Facilities = g.GiteFacilities.Select(gf => gf.Facility!.Name)
                })
                .FirstOrDefaultAsync();

            if (gite == null)
                return NotFound($"Gîte met id {id} niet gevonden.");

            return Ok(gite);
        }

        // POST /gites — nieuwe gîte aanmaken
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGiteDto dto)
        {
            var gite = new Gite
            {
                Name = dto.Name,
                Description = dto.Description,
                MaxOccupancy = dto.MaxOccupancy,
                PricePerNight = dto.PricePerNight,
                IsAvailable = dto.IsAvailable
            };

            _context.Gites.Add(gite);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = gite.Id }, gite);
        }

        // PUT /gites/{id} — gîte bijwerken
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateGiteDto dto)
        {
            var gite = await _context.Gites.FindAsync(id);
            if (gite == null)
                return NotFound($"Gîte met id {id} niet gevonden.");

            if (dto.Name != null) gite.Name = dto.Name;
            if (dto.Description != null) gite.Description = dto.Description;
            if (dto.MaxOccupancy.HasValue) gite.MaxOccupancy = dto.MaxOccupancy.Value;
            if (dto.PricePerNight.HasValue) gite.PricePerNight = dto.PricePerNight.Value;
            if (dto.IsAvailable.HasValue) gite.IsAvailable = dto.IsAvailable.Value;
            gite.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(gite);
        }

        // DELETE /gites/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var gite = await _context.Gites.FindAsync(id);
            if (gite == null)
                return NotFound($"Gîte met id {id} niet gevonden.");

            _context.Gites.Remove(gite);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
