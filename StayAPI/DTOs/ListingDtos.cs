using System.ComponentModel.DataAnnotations;

namespace StayAPI.DTOs;

public class InsertListingDto
{
    [Required]
    [Range(1, 50)]
    public int NoOfPeople { get; set; }

    [Required]
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [Range(1, 100000)]
    public decimal Price { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
}

public class QueryListingsDto
{
    [Required]
    public DateTime DateFrom { get; set; }

    [Required]
    public DateTime DateTo { get; set; }

    [Required]
    [Range(1, 50)]
    public int NoOfPeople { get; set; }

    [Required]
    public string Country { get; set; } = string.Empty;

    [Required]
    public string City { get; set; } = string.Empty;

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class ListingResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int NoOfPeople { get; set; }
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }
}

public class StatusResponseDto
{
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
    public int? Id { get; set; }
}

public class PagedResponseDto<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public class FileUploadResponseDto
{
    public string Status { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
