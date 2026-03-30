using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayAPI.DTOs;
using StayAPI.Services;

namespace StayAPI.Controllers.v1;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    [Authorize(Roles = "Guest,Admin")]
    [ProducesResponseType(typeof(StatusResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ReviewStay([FromBody] ReviewStayDto dto)
    {
        var userId = GetUserId();
        var result = await _reviewService.ReviewStay(dto, userId);

        if (result.Status == "Error")
            return BadRequest(result);

        return Ok(result);
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim!);
    }
}
