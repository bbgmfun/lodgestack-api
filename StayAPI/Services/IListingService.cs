using StayAPI.DTOs;

namespace StayAPI.Services;

public interface IListingService
{
    Task<StatusResponseDto> InsertListing(InsertListingDto dto, int hostUserId);
    Task<PagedResponseDto<ListingResponseDto>> QueryListings(QueryListingsDto dto);
    Task<PagedResponseDto<ListingReportItemDto>> ReportListings(ReportListingsDto dto);
    Task<FileUploadResponseDto> InsertListingsByFile(Stream fileStream, int hostUserId);
}
