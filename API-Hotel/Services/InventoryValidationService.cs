using hotel.Data;
using hotel.DTOs.Common;
using hotel.DTOs.Inventory;
using hotel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace hotel.Services;

/// <summary>
/// Service focused solely on inventory validation operations
/// Follows Single Responsibility Principle
/// </summary>
public class InventoryValidationService : IInventoryValidationService
{
    private readonly HotelDbContext _context;
    private readonly ILogger<InventoryValidationService> _logger;

    public InventoryValidationService(
        HotelDbContext context,
        ILogger<InventoryValidationService> logger
    )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<StockValidationDto>> ValidateStockAsync(
        int articuloId,
        int requestedQuantity,
        InventoryLocationType locationType,
        int? locationId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var query = _context
                .InventarioUnificado.AsNoTracking()
                .Where(i =>
                    i.ArticuloId == articuloId
                    && i.InstitucionID == institucionId
                    && i.TipoUbicacion == (int)locationType
                );

            if (locationId.HasValue)
            {
                query = query.Where(i => i.UbicacionId == locationId.Value);
            }

            var inventory = await query.FirstOrDefaultAsync(cancellationToken);

            var validation = new StockValidationDto
            {
                ArticuloId = articuloId,
                RequestedQuantity = requestedQuantity,
                AvailableQuantity = inventory?.Cantidad ?? 0,
                IsValid = (inventory?.Cantidad ?? 0) >= requestedQuantity,
                LocationType = locationType,
                LocationId = locationId,
            };

            _logger.LogDebug(
                "Stock validation for article {ArticuloId}: Requested {RequestedQuantity}, Available {AvailableQuantity}, Valid {IsValid}",
                articuloId,
                requestedQuantity,
                validation.AvailableQuantity,
                validation.IsValid
            );

            return ApiResponse<StockValidationDto>.Success(validation);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error validating stock for article {ArticuloId} in institution {InstitucionId}",
                articuloId,
                institucionId
            );
            return ApiResponse<StockValidationDto>.Failure("Error validating stock");
        }
    }

    public async Task<ApiResponse<IEnumerable<StockValidationDto>>> ValidateMultipleStockAsync(
        IEnumerable<StockValidationRequestDto> requests,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var validations = new List<StockValidationDto>();

            foreach (var request in requests)
            {
                var result = await ValidateStockAsync(
                    request.ArticuloId,
                    request.RequestedQuantity,
                    request.LocationType,
                    request.LocationId,
                    institucionId,
                    cancellationToken
                );

                if (result.IsSuccess && result.Data != null)
                {
                    validations.Add(result.Data);
                }
            }

            return ApiResponse<IEnumerable<StockValidationDto>>.Success(validations);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error validating multiple stock items for institution {InstitucionId}",
                institucionId
            );
            return ApiResponse<IEnumerable<StockValidationDto>>.Failure("Error validating multiple stock items");
        }
    }

    public async Task<ApiResponse<bool>> ValidateAvailabilityAsync(
        int articuloId,
        int requiredQuantity,
        InventoryLocationType locationType,
        int? locationId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var validationResult = await ValidateStockAsync(
                articuloId,
                requiredQuantity,
                locationType,
                locationId,
                institucionId,
                cancellationToken
            );

            if (!validationResult.IsSuccess)
            {
                return ApiResponse<bool>.Failure(validationResult.Errors, validationResult.Message);
            }

            return ApiResponse<bool>.Success(validationResult.Data!.IsValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error validating availability for article {ArticuloId} in institution {InstitucionId}",
                articuloId,
                institucionId
            );
            return ApiResponse<bool>.Failure("Error validating availability");
        }
    }

    public async Task<ApiResponse<int>> GetAvailableQuantityAsync(
        int articuloId,
        InventoryLocationType locationType,
        int? locationId,
        int institucionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var query = _context
                .InventarioUnificado.AsNoTracking()
                .Where(i =>
                    i.ArticuloId == articuloId
                    && i.InstitucionID == institucionId
                    && i.TipoUbicacion == (int)locationType
                );

            if (locationId.HasValue)
            {
                query = query.Where(i => i.UbicacionId == locationId.Value);
            }

            var inventory = await query.FirstOrDefaultAsync(cancellationToken);
            var availableQuantity = inventory?.Cantidad ?? 0;

            _logger.LogDebug(
                "Available quantity for article {ArticuloId} in location {LocationType}:{LocationId} is {Quantity}",
                articuloId,
                locationType,
                locationId,
                availableQuantity
            );

            return ApiResponse<int>.Success(availableQuantity);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting available quantity for article {ArticuloId} in institution {InstitucionId}",
                articuloId,
                institucionId
            );
            return ApiResponse<int>.Failure("Error getting available quantity");
        }
    }
}