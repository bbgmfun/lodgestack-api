using StayAPI.DTOs;

namespace StayAPI.Services;

public interface IReviewService
{
    Task<StatusResponseDto> ReviewStay(ReviewStayDto dto, int guestUserId);
}
