namespace LeMarconnesGiteAPI.Models
{
    public class GiteFacility
    {
        public int GiteId { get; set; }
        public int FacilityId { get; set; }

        // Navigation
        public Gite Gite { get; set; } = null!;
        public Facility Facility { get; set; } = null!;
    }
}
