namespace LeMarconnesGiteAPI.DTOs
{
    // POST /gites
    public class CreateGiteDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }        // optioneel
        public int MaxOccupancy { get; set; }
        public decimal PricePerNight { get; set; }
        public bool IsAvailable { get; set; } = true;  // optioneel, default true
    }

    // PUT /gites/{id}
    public class UpdateGiteDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? MaxOccupancy { get; set; }
        public decimal? PricePerNight { get; set; }
        public bool? IsAvailable { get; set; }
    }
}
