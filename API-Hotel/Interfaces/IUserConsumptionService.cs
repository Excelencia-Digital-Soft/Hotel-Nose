using hotel.DTOs.Common;
using hotel.DTOs.UserConsumption;

namespace hotel.Interfaces;

public interface IUserConsumptionService
{
    Task<ApiResponse<IEnumerable<UserConsumptionDto>>> GetUserConsumptionAsync(
        string userId,
        int institucionId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<UserConsumptionSummaryDto>> GetUserConsumptionSummaryAsync(
        string userId,
        int institucionId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<IEnumerable<UserConsumptionDto>>> GetAllUsersConsumptionAsync(
        int institucionId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<UserConsumptionDto>> RegisterConsumptionAsync(
        UserConsumptionCreateDto createDto,
        string userId,
        int institucionId,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<IEnumerable<UserConsumptionByServiceDto>>> GetConsumptionByServiceAsync(
        string userId,
        int institucionId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);
    
    Task<ApiResponse<UserConsumptionDto>> AdminCreateConsumptionForUserAsync(
        AdminUserConsumptionCreateDto createDto,
        int institucionId,
        string adminUserId,
        CancellationToken cancellationToken = default);
}