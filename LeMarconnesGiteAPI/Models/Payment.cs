namespace LeMarconnesGiteAPI.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Openstaand";

        // Navigation
        public Reservation Reservation { get; set; } = null!;
    }
}
