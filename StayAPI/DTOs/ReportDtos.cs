using System.ComponentModel.DataAnnotations;

namespace StayAPI.DTOs;

public class ReportListingsDto
{
    public string? Country { get; set; }
    public string? City { get; set; }

    [Range(1, 5)]
    public double? MinRating { get; set; }

    [Range(1, 5)]
    public double? MaxRating { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class ListingReportItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int NoOfPeople { get; set; }
    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int BookingCount { get; set; }
}
