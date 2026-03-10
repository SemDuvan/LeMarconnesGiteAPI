namespace LeMarconnesGiteAPI.DTOs
{
    // POST /guests
    public class CreateGuestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }  // optioneel
    }

    // PUT /guests/{id}
    public class UpdateGuestDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}
