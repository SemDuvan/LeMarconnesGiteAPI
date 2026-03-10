namespace LeMarconnesGiteAPI.Models
{
    public class Gite
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MaxOccupancy { get; set; }
        public decimal PricePerNight { get; set; }
        public bool IsAvailable { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<GiteFacility> GiteFacilities { get; set; } = new List<GiteFacility>();
        public ICollection<Pricing> Pricings { get; set; } = new List<Pricing>();
    }
}
