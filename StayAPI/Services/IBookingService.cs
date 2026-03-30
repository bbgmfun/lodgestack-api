using StayAPI.DTOs;

namespace StayAPI.Services;

public interface IBookingService
{
    Task<StatusResponseDto> BookStay(BookStayDto dto, int guestUserId);
}
