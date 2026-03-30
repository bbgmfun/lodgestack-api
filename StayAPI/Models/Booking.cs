namespace StayAPI.Models;

public class Booking
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public int GuestUserId { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public string NamesOfPeople { get; set; } = string.Empty;
    public string Status { get; set; } = "Confirmed";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Listing? Listing { get; set; }
    public User? GuestUser { get; set; }
    public Review? Review { get; set; }
}
