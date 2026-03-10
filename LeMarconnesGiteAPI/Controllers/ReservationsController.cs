using LeMarconnesGiteAPI.Data;
using LeMarconnesGiteAPI.DTOs;
using LeMarconnesGiteAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeMarconnesGiteAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReservationsController(AppDbContext context)
        {
            _context = context;
        }

        // GET /reservations
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reservations = await _context.Reservations
                .Include(r => r.Gite)
                .Include(r => r.Guest)
                .Select(r => new
                {
                    r.Id,
                    Gite = r.Gite.Name,
                    Guest = r.Guest.Name,
                    r.StartDate,
                    r.EndDate,
                    r.TotalPrice,
                    r.Status,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(reservations);
        }

        // GET /reservations/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Gite)
                .Include(r => r.Guest)
                .Where(r => r.Id == id)
                .Select(r => new
                {
                    r.Id,
                    Gite = r.Gite.Name,
                    Guest = r.Guest.Name,
                    r.StartDate,
                    r.EndDate,
                    r.TotalPrice,
                    r.Status,
                    r.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (reservation == null)
                return NotFound($"Reservering met id {id} niet gevonden.");

            return Ok(reservation);
        }

        // FR-06 + FR-07: POST /reservations
        // TotalPrice wordt automatisch berekend: seizoensprijs OF standaard prijs × nachten
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
        {
            var gite = await _context.Gites.FindAsync(dto.GiteId);
            if (gite == null)
                return NotFound($"Gîte met id {dto.GiteId} niet gevonden.");

            var guest = await _context.Guests.FindAsync(dto.GuestId);
            if (guest == null)
                return NotFound($"Gast met id {dto.GuestId} niet gevonden.");

            if (dto.EndDate <= dto.StartDate)
                return BadRequest("Einddatum moet na de startdatum liggen.");

            // Controleer of de gîte niet al bezet is in die periode (FR-02)
            bool isOccupied = await _context.Reservations.AnyAsync(r =>
                r.GiteId == dto.GiteId &&
                r.Status != "Geannuleerd" &&
                r.StartDate < dto.EndDate &&
                r.EndDate > dto.StartDate);

            if (isOccupied)
                return Conflict("De gîte is al bezet in de opgegeven periode.");

            // FR-07: Prijsberekening — zoek seizoensprijs, anders standaardprijs
            int nights = (int)(dto.EndDate - dto.StartDate).TotalDays;
            var seizoensPrijs = await _context.Pricings.FirstOrDefaultAsync(p =>
                p.GiteId == dto.GiteId &&
                p.StartDate <= dto.StartDate &&
                p.EndDate >= dto.EndDate);

            decimal pricePerNight = seizoensPrijs?.PricePerNight ?? gite.PricePerNight;
            decimal totalPrice = pricePerNight * nights;

            var reservation = new Reservation
            {
                GiteId = dto.GiteId,
                GuestId = dto.GuestId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                TotalPrice = totalPrice,
                Status = "Bevestigd"
            };

            _context.Reservations.Add(reservation);

            // FR-09: Audit log aanmaken
            _context.AuditLogs.Add(new AuditLog
            {
                EntityName = "Reservation",
                Action = "Created",
                Description = $"Reservering aangemaakt voor gîte '{gite.Name}', gast '{guest.Name}', " +
                              $"{dto.StartDate:dd-MM-yyyy} t/m {dto.EndDate:dd-MM-yyyy}, totaalprijs: €{totalPrice:F2}"
            });

            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, new
            {
                reservation.Id,
                reservation.GiteId,
                reservation.GuestId,
                reservation.StartDate,
                reservation.EndDate,
                reservation.TotalPrice,
                reservation.Status
            });
        }

        // FR-06 + FR-09: PUT /reservations/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReservationDto dto)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Gite)
                .Include(r => r.Guest)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return NotFound($"Reservering met id {id} niet gevonden.");

            var changes = new List<string>();

            if (dto.StartDate.HasValue)
            {
                changes.Add($"StartDate: {reservation.StartDate:dd-MM-yyyy} → {dto.StartDate.Value:dd-MM-yyyy}");
                reservation.StartDate = dto.StartDate.Value;
            }

            if (dto.EndDate.HasValue)
            {
                changes.Add($"EndDate: {reservation.EndDate:dd-MM-yyyy} → {dto.EndDate.Value:dd-MM-yyyy}");
                reservation.EndDate = dto.EndDate.Value;
            }

            if (dto.Status != null)
            {
                changes.Add($"Status: {reservation.Status} → {dto.Status}");
                reservation.Status = dto.Status;
            }

            // Herbereken prijs als datums gewijzigd zijn
            if (dto.StartDate.HasValue || dto.EndDate.HasValue)
            {
                int nights = (int)(reservation.EndDate - reservation.StartDate).TotalDays;
                var seizoensPrijs = await _context.Pricings.FirstOrDefaultAsync(p =>
                    p.GiteId == reservation.GiteId &&
                    p.StartDate <= reservation.StartDate &&
                    p.EndDate >= reservation.EndDate);

                decimal pricePerNight = seizoensPrijs?.PricePerNight ?? reservation.Gite.PricePerNight;
                reservation.TotalPrice = pricePerNight * nights;
                changes.Add($"TotalPrice herberekend: €{reservation.TotalPrice:F2}");
            }

            // FR-09: Audit log
            _context.AuditLogs.Add(new AuditLog
            {
                EntityName = "Reservation",
                Action = "Updated",
                Description = $"Reservering {id} bijgewerkt: {string.Join(", ", changes)}"
            });

            await _context.SaveChangesAsync();
            return Ok(reservation);
        }

        // FR-06 + FR-09: DELETE /reservations/{id} — zet status op Geannuleerd
        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Gite)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return NotFound($"Reservering met id {id} niet gevonden.");

            reservation.Status = "Geannuleerd";

            // FR-09: Audit log
            _context.AuditLogs.Add(new AuditLog
            {
                EntityName = "Reservation",
                Action = "Cancelled",
                Description = $"Reservering {id} geannuleerd (gîte: '{reservation.Gite.Name}', " +
                              $"{reservation.StartDate:dd-MM-yyyy} t/m {reservation.EndDate:dd-MM-yyyy})"
            });

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Reservering {id} is geannuleerd.", reservation.Status });
        }
    }
}
