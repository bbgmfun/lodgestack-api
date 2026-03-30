namespace StayAPI.Models;

public class Review
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int ListingId { get; set; }
    public int GuestUserId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Booking? Booking { get; set; }
    public Listing? Listing { get; set; }
    public User? GuestUser { get; set; }
}
