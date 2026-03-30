using Microsoft.EntityFrameworkCore;
using StayAPI.Data;
using StayAPI.DTOs;
using StayAPI.Models;

namespace StayAPI.Services;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;

    public ReviewService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<StatusResponseDto> ReviewStay(ReviewStayDto dto, int guestUserId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Listing)
            .FirstOrDefaultAsync(b => b.Id == dto.BookingId);

        if (booking == null)
            return new StatusResponseDto { Status = "Error", Message = "Booking not found" };

        if (booking.GuestUserId != guestUserId)
            return new StatusResponseDto { Status = "Error", Message = "You can only review your own bookings" };

        var existingReview = await _context.Reviews.AnyAsync(r => r.BookingId == dto.BookingId);
        if (existingReview)
            return new StatusResponseDto { Status = "Error", Message = "This booking has already been reviewed" };

        if (dto.Rating < 1 || dto.Rating > 5)
            return new StatusResponseDto { Status = "Error", Message = "Rating must be between 1 and 5" };

        var review = new Review
        {
            BookingId = dto.BookingId,
            ListingId = booking.ListingId,
            GuestUserId = guestUserId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return new StatusResponseDto
        {
            Status = "Successful",
            Message = "Review submitted successfully",
            Id = review.Id
        };
    }
}
