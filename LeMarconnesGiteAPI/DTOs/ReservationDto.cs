namespace LeMarconnesGiteAPI.DTOs
{
    // POST /reservations
    // TotalPrice en Status worden automatisch ingevuld door de API
    public class CreateReservationDto
    {
        public int GiteId { get; set; }
        public int GuestId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal DepositAmount { get; set; } = 0;  // optioneel, default geen aanbetaling
    }

    // PUT /reservations/{id}
    public class UpdateReservationDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }      // bijv. "Geannuleerd"
        public bool? DepositPaid { get; set; }   // markeer aanbetaling als betaald
    }
}
