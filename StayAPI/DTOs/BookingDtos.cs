using System.ComponentModel.DataAnnotations;

namespace StayAPI.DTOs;

public class BookStayDto
{
    [Required]
    public int ListingId { get; set; }

    [Required]
    public DateTime DateFrom { get; set; }

    [Required]
    public DateTime DateTo { get; set; }

    [Required]
    [MinLength(1)]
    public List<string> NamesOfPeople { get; set; } = new();
}

public class BookingResponseDto
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public string ListingTitle { get; set; } = string.Empty;
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public List<string> NamesOfPeople { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
