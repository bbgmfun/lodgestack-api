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
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    [Authorize(Roles = "Guest,Admin")]
    [ProducesResponseType(typeof(StatusResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> BookStay([FromBody] BookStayDto dto)
    {
        var userId = GetUserId();
        var result = await _bookingService.BookStay(dto, userId);

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
