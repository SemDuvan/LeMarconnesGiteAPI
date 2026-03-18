using LeMarconnesGiteAPI.Data;
using LeMarconnesGiteAPI.DTOs;
using LeMarconnesGiteAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeMarconnesGiteAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET /payments
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var payments = await _context.Payments
                .Include(p => p.Reservation)
                .Select(p => new
                {
                    p.Id,
                    p.ReservationId,
                    p.Amount,
                    p.PaymentDate,
                    p.Status
                })
                .ToListAsync();

            return Ok(payments);
        }

        // GET /payments/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return NotFound($"Betaling met id {id} niet gevonden.");

            return Ok(new { payment.Id, payment.ReservationId, payment.Amount, payment.PaymentDate, payment.Status });
        }

        // POST /payments
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
        {
            var reservation = await _context.Reservations.FindAsync(dto.ReservationId);
            if (reservation == null)
                return NotFound($"Reservering met id {dto.ReservationId} niet gevonden.");

            var payment = new Payment
            {
                ReservationId = dto.ReservationId,
                Amount = dto.Amount,
                Status = dto.Status
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, new
            {
                payment.Id,
                payment.ReservationId,
                payment.Amount,
                payment.PaymentDate,
                payment.Status
            });
        }

        // PUT /payments/{id} — status bijwerken (bijv. "Betaald")
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdatePaymentDto dto)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return NotFound($"Betaling met id {id} niet gevonden.");

            if (dto.Status != null)
                payment.Status = dto.Status;

            await _context.SaveChangesAsync();
            return Ok(new { payment.Id, payment.ReservationId, payment.Amount, payment.PaymentDate, payment.Status });
        }
    }
}
