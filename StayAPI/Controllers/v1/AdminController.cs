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
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IListingService _listingService;

    public AdminController(IListingService listingService)
    {
        _listingService = listingService;
    }

    [HttpGet("report/listings")]
    [ProducesResponseType(typeof(PagedResponseDto<ListingReportItemDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ReportListings([FromQuery] ReportListingsDto dto)
    {
        var result = await _listingService.ReportListings(dto);
        return Ok(result);
    }

    [HttpPost("listings/upload")]
    [ProducesResponseType(typeof(FileUploadResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> InsertListingByFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded" });

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Only CSV files are accepted" });

        var userId = GetUserId();
        using var stream = file.OpenReadStream();
        var result = await _listingService.InsertListingsByFile(stream, userId);

        return Ok(result);
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim!);
    }
}
