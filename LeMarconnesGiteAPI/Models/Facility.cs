namespace LeMarconnesGiteAPI.Models
{
    public class Facility
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Navigation
        public ICollection<GiteFacility> GiteFacilities { get; set; } = new List<GiteFacility>();
    }
}
