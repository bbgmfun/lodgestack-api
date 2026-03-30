using System.ComponentModel.DataAnnotations;

namespace StayAPI.DTOs;

public class ReviewStayDto
{
    [Required]
    public int BookingId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;
}

public class ReviewResponseDto
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int ListingId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
