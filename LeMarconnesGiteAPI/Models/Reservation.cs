namespace LeMarconnesGiteAPI.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int GiteId { get; set; }
        public int GuestId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Bevestigd";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public Gite Gite { get; set; } = null!;
        public Guest Guest { get; set; } = null!;
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
