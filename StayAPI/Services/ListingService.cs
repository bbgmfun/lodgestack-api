using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using StayAPI.Data;
using StayAPI.DTOs;
using StayAPI.Models;

namespace StayAPI.Services;

public class ListingService : IListingService
{
    private readonly AppDbContext _context;

    public ListingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<StatusResponseDto> InsertListing(InsertListingDto dto, int hostUserId)
    {
        var listing = new Listing
        {
            HostUserId = hostUserId,
            Title = string.IsNullOrWhiteSpace(dto.Title) ? $"Stay in {dto.City}" : dto.Title,
            Description = dto.Description,
            NoOfPeople = dto.NoOfPeople,
            Country = dto.Country,
            City = dto.City,
            Price = dto.Price,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Listings.Add(listing);
        await _context.SaveChangesAsync();

        return new StatusResponseDto
        {
            Status = "Successful",
            Message = "Listing created successfully",
            Id = listing.Id
        };
    }

    public async Task<PagedResponseDto<ListingResponseDto>> QueryListings(QueryListingsDto dto)
    {
        var dateFrom = NormalizeDate(dto.DateFrom);
        var dateTo = NormalizeDate(dto.DateTo);

        var bookedListingIds = await _context.Bookings
            .Where(b => b.Status == "Confirmed" &&
                        b.DateFrom < dateTo &&
                        b.DateTo > dateFrom)
            .Select(b => b.ListingId)
            .Distinct()
            .ToListAsync();

        var query = _context.Listings
            .Where(l => l.IsActive &&
                        l.Country.ToLower() == dto.Country.ToLower() &&
                        l.City.ToLower() == dto.City.ToLower() &&
                        l.NoOfPeople >= dto.NoOfPeople &&
                        !bookedListingIds.Contains(l.Id))
            .Include(l => l.Reviews);

        var totalCount = await query.CountAsync();

        var listings = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((dto.Page - 1) * dto.PageSize)
            .Take(dto.PageSize)
            .Select(l => new ListingResponseDto
            {
                Id = l.Id,
                Title = l.Title,
                Description = l.Description,
                NoOfPeople = l.NoOfPeople,
                Country = l.Country,
                City = l.City,
                Price = l.Price,
                AverageRating = l.Reviews.Any() ? l.Reviews.Average(r => r.Rating) : null,
                ReviewCount = l.Reviews.Count
            })
            .ToListAsync();

        return new PagedResponseDto<ListingResponseDto>
        {
            Items = listings,
            Page = dto.Page,
            PageSize = dto.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)dto.PageSize)
        };
    }

    private static DateTime NormalizeDate(DateTime value)
    {
        return DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
    }

    public async Task<PagedResponseDto<ListingReportItemDto>> ReportListings(ReportListingsDto dto)
    {
        var query = _context.Listings
            .Include(l => l.Reviews)
            .Include(l => l.Bookings)
            .Where(l => l.IsActive);

        if (!string.IsNullOrWhiteSpace(dto.Country))
            query = query.Where(l => l.Country.ToLower() == dto.Country.ToLower());

        if (!string.IsNullOrWhiteSpace(dto.City))
            query = query.Where(l => l.City.ToLower() == dto.City.ToLower());

        if (dto.MinRating.HasValue)
            query = query.Where(l => l.Reviews.Any() &&
                                     l.Reviews.Average(r => r.Rating) >= dto.MinRating.Value);

        if (dto.MaxRating.HasValue)
            query = query.Where(l => l.Reviews.Any() &&
                                     l.Reviews.Average(r => r.Rating) <= dto.MaxRating.Value);

        var totalCount = await query.CountAsync();

        var listings = await query
            .OrderByDescending(l => l.Reviews.Any() ? l.Reviews.Average(r => r.Rating) : 0)
            .Skip((dto.Page - 1) * dto.PageSize)
            .Take(dto.PageSize)
            .Select(l => new ListingReportItemDto
            {
                Id = l.Id,
                Title = l.Title,
                Country = l.Country,
                City = l.City,
                Price = l.Price,
                NoOfPeople = l.NoOfPeople,
                AverageRating = l.Reviews.Any() ? l.Reviews.Average(r => r.Rating) : null,
                ReviewCount = l.Reviews.Count,
                BookingCount = l.Bookings.Count
            })
            .ToListAsync();

        return new PagedResponseDto<ListingReportItemDto>
        {
            Items = listings,
            Page = dto.Page,
            PageSize = dto.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)dto.PageSize)
        };
    }

    public async Task<FileUploadResponseDto> InsertListingsByFile(Stream fileStream, int hostUserId)
    {
        var response = new FileUploadResponseDto();
    
        try
        {
            using var reader = new StreamReader(fileStream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                HasHeaderRecord = true
            });

            var csvRecords = csv.GetRecords<CsvListingRecord>().ToList();
            response.TotalRecords = csvRecords.Count;

            foreach (var record in csvRecords)
            {
                try
                {
                    var listing = new Listing
                    {
                        HostUserId = hostUserId,
                        Title = string.IsNullOrWhiteSpace(record.Title) ? $"Stay in {record.City}" : record.Title,
                        NoOfPeople = record.NoOfPeople,
                        Country = record.Country,
                        City = record.City,
                        Price = record.Price,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    _context.Listings.Add(listing);
                    response.SuccessCount++;
                }
                catch (Exception ex)
                {
                    response.ErrorCount++;
                    response.Errors.Add($"Row error: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            response.Status = response.ErrorCount == 0 ? "Successful" : "Partial";
        }
        catch (Exception ex)
        {
            response.Status = "Error";
            response.Errors.Add($"File processing error: {ex.Message}");
        }

        return response;
    }
}

public class CsvListingRecord
{
    public int NoOfPeople { get; set; }
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Title { get; set; } = string.Empty;
}
