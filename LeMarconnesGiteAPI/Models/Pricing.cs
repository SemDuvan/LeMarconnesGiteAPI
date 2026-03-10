namespace LeMarconnesGiteAPI.Models
{
    public class Pricing
    {
        public int Id { get; set; }
        public int GiteId { get; set; }
        public string? Season { get; set; }
        public decimal PricePerNight { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Navigation
        public Gite Gite { get; set; } = null!;
    }
}
