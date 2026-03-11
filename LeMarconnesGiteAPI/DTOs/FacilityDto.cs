namespace LeMarconnesGiteAPI.DTOs
{
    // POST /gites/{id}/facilities — faciliteit toevoegen op naam (aanmaken of bestaande gebruiken)
    public class AddFacilityToGiteDto
    {
        public string Name { get; set; } = string.Empty;
    }
}
