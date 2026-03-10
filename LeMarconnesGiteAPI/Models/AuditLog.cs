namespace LeMarconnesGiteAPI.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string EntityName { get; set; } = string.Empty;   // bijv. "Reservation"
        public string Action { get; set; } = string.Empty;       // bijv. "Created", "Updated", "Cancelled"
        public string Description { get; set; } = string.Empty;  // vrije tekst wat er is veranderd
        public DateTime ChangedAt { get; set; } = DateTime.Now;
    }
}
