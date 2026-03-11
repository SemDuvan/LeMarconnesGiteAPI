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

        // GET /gites/personen-opties — zonder parameter: toon beschikbare aantallen
        //                             — met ?personen=4: toon gîtes die passen
        [HttpGet("personen-opties")]
        public async Task<IActionResult> GetPersonenOpties([FromQuery] int? personen)
        {
            if (!personen.HasValue)
            {
                // Geen getal ingevuld → toon welke aantallen beschikbaar zijn
                var opties = await _context.Gites
                    .Select(g => g.MaxOccupancy)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToListAsync();

                return Ok(new
                {
                    Uitleg = "Voer ?personen=<aantal> in om gîtes te zien die bij dat aantal passen.",
                    BeschikbareAantallen = opties
                });
            }

            // Getal ingevuld → filter gîtes op MaxOccupancy
            var gites = await _context.Gites
                .Include(g => g.GiteFacilities)
                    .ThenInclude(gf => gf.Facility)
                .Where(g => g.MaxOccupancy >= personen.Value && g.IsAvailable)
                .Select(g => new
                {
                    g.Id,
                    g.Name,
                    g.Description,
                    g.MaxOccupancy,
                    g.PricePerNight,
                    Facilities = g.GiteFacilities.Select(gf => gf.Facility!.Name)
                })
                .ToListAsync();

            if (!gites.Any())
                return NotFound($"Geen beschikbare gîtes gevonden voor {personen.Value} personen.");

            return Ok(gites);
        }

        // POST /gites/{id}/facilities — faciliteit koppelen op naam (bestaande gebruiken of nieuw aanmaken)
        [HttpPost("{id}/facilities")]
        public async Task<IActionResult> AddFacility(int id, [FromBody] AddFacilityToGiteDto dto)
        {
            var gite = await _context.Gites.FindAsync(id);
            if (gite == null)
                return NotFound($"Gîte met id {id} niet gevonden.");

            // Zoek faciliteit op naam (hoofdletterongevoelig), maak aan als die nog niet bestaat
            var facilityNaam = dto.Name.Trim();
            var facility = await _context.Facilities
                .FirstOrDefaultAsync(f => f.Name.ToLower() == facilityNaam.ToLower());

            if (facility == null)
            {
                facility = new Facility { Name = facilityNaam };
                _context.Facilities.Add(facility);
                await _context.SaveChangesAsync(); // ID genereren
            }

            // Controleer of de koppeling al bestaat
            var bestaatAl = await _context.GiteFacilities
                .AnyAsync(gf => gf.GiteId == id && gf.FacilityId == facility.Id);

            if (bestaatAl)
                return Conflict($"Faciliteit '{facility.Name}' is al gekoppeld aan deze gîte.");

            _context.GiteFacilities.Add(new GiteFacility
            {
                GiteId = id,
                FacilityId = facility.Id
            });

            await _context.SaveChangesAsync();
            return Ok($"Faciliteit '{facility.Name}' gekoppeld aan gîte '{gite.Name}'.");
        }

        // Fix 2: DELETE /gites/{id}/facilities/{facilityId} — faciliteit ontkoppelen
        [HttpDelete("{id}/facilities/{facilityId}")]
        public async Task<IActionResult> RemoveFacility(int id, int facilityId)
        {
            var koppeling = await _context.GiteFacilities
                .FirstOrDefaultAsync(gf => gf.GiteId == id && gf.FacilityId == facilityId);

            if (koppeling == null)
                return NotFound("Deze faciliteit is niet gekoppeld aan de gîte.");

            _context.GiteFacilities.Remove(koppeling);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
