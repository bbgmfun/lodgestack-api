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
public class ListingsController : ControllerBase
{
    private readonly IListingService _listingService;

    public ListingsController(IListingService listingService)
    {
        _listingService = listingService;
    }

    [HttpPost]
    [Authorize(Roles = "Host,Admin")]
    [ProducesResponseType(typeof(StatusResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> InsertListing([FromBody] InsertListingDto dto)
    {
        var userId = GetUserId();
        var result = await _listingService.InsertListing(dto, userId);
        return Ok(result);
    }

    [HttpGet("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResponseDto<ListingResponseDto>), 200)]
    public async Task<IActionResult> QueryListings([FromQuery] QueryListingsDto dto)
    {
        var result = await _listingService.QueryListings(dto);
        return Ok(result);
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim!);
    }
}
