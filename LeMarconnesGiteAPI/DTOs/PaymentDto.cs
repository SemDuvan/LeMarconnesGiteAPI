namespace LeMarconnesGiteAPI.DTOs
{
    // POST /payments
    public class CreatePaymentDto
    {
        public int ReservationId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Openstaand";  // optioneel, default Openstaand
    }

    // PUT /payments/{id}
    public class UpdatePaymentDto
    {
        public string? Status { get; set; }  // bijv. "Betaald"
    }
}
