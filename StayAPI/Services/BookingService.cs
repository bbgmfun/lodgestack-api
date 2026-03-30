using Microsoft.EntityFrameworkCore;
using StayAPI.Data;
using StayAPI.DTOs;
using StayAPI.Models;

namespace StayAPI.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;

    public BookingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<StatusResponseDto> BookStay(BookStayDto dto, int guestUserId)
    {
        var dateFrom = NormalizeDate(dto.DateFrom);
        var dateTo = NormalizeDate(dto.DateTo);

        var listing = await _context.Listings.FindAsync(dto.ListingId);
        if (listing == null || !listing.IsActive)
            return new StatusResponseDto { Status = "Error", Message = "Listing not found or inactive" };

        if (dateFrom >= dateTo)
            return new StatusResponseDto { Status = "Error", Message = "DateFrom must be before DateTo" };

        if (dateFrom < DateTime.UtcNow.Date)
            return new StatusResponseDto { Status = "Error", Message = "Cannot book dates in the past" };

        if (dto.NamesOfPeople.Count > listing.NoOfPeople)
            return new StatusResponseDto { Status = "Error", Message = $"Listing supports max {listing.NoOfPeople} people" };

        var isBooked = await _context.Bookings.AnyAsync(b =>
            b.ListingId == dto.ListingId &&
            b.Status == "Confirmed" &&
            b.DateFrom < dateTo &&
            b.DateTo > dateFrom);

        if (isBooked)
            return new StatusResponseDto { Status = "Error", Message = "Listing is already booked for the selected dates" };

        var booking = new Booking
        {
            ListingId = dto.ListingId,
            GuestUserId = guestUserId,
            DateFrom = dateFrom,
            DateTo = dateTo,
            NamesOfPeople = string.Join(", ", dto.NamesOfPeople),
            Status = "Confirmed",
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return new StatusResponseDto
        {
            Status = "Successful",
            Message = "Booking confirmed",
            Id = booking.Id
        };
    }

    private static DateTime NormalizeDate(DateTime value)
    {
        return DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
    }
}
