// =============================================
// EJEMPLO: Corrección del uso de UserId en controladores
// =============================================

using System.Security.Claims;
using hotel.Extensions;
using hotel.DTOs.Common;
using Microsoft.AspNetCore.Mvc;

namespace hotel.Examples;

/// <summary>
/// Ejemplo de cómo corregir el uso de UserId en controladores
/// pasando de int (legacy) a string (AspNetCore Identity)
/// </summary>
public class CorrectUserIdUsageExample : ControllerBase
{
    // ❌ CÓDIGO INCORRECTO (Legacy - usando int)
    public async Task<IActionResult> IncorrectWay_CancelReservation(int reservaId)
    {
        // ❌ MAL: Intenta parsear el UserId como int
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return BadRequest(ApiResponse.Failure("User ID is required"));
        }

        // ❌ MAL: Pasa userId como int al servicio
        // var result = await _reservasService.ComprehensiveCancelOccupationAsync(
        //     reservaId, 
        //     cancelDto.Reason, 
        //     institucionId.Value,
        //     userId,  // ❌ int en lugar de string
        //     cancellationToken);

        return Ok();
    }

    // ✅ CÓDIGO CORRECTO (AspNetCore Identity - usando string)
    public async Task<IActionResult> CorrectWay_CancelReservation(int reservaId)
    {
        // ✅ CORRECTO: Obtiene el UserId como string directamente
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(ApiResponse.Failure("User ID is required"));
        }

        var institucionId = this.GetCurrentInstitucionId();
        if (!institucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        // ✅ CORRECTO: Pasa userId como string al servicio
        // var result = await _reservasService.ComprehensiveCancelOccupationAsync(
        //     reservaId, 
        //     cancelDto.Reason, 
        //     institucionId.Value,
        //     userId,  // ✅ string (GUID) de AspNetCore Identity
        //     cancellationToken);

        return Ok();
    }

    // ✅ MEJOR AÚN: Usando extension methods para mayor limpieza
    public async Task<IActionResult> BestWay_CancelReservation(int reservaId)
    {
        var currentUser = this.GetCurrentUserInfo();
        if (currentUser == null)
        {
            return BadRequest(ApiResponse.Failure("Authentication required"));
        }

        if (!currentUser.InstitucionId.HasValue)
        {
            return BadRequest(ApiResponse.Failure("Institution ID is required"));
        }

        // ✅ MEJOR: Usando la información completa del usuario
        // var result = await _reservasService.ComprehensiveCancelOccupationAsync(
        //     reservaId, 
        //     cancelDto.Reason, 
        //     currentUser.InstitucionId.Value,
        //     currentUser.UserId,  // ✅ string desde extension method
        //     cancellationToken);

        // ✅ También disponible para auditoría
        // await _registrosService.LogAuditAsync(
        //     $"Reserva {reservaId} cancelada por usuario {currentUser.UserName}",
        //     ModuloSistema.RESERVAS,
        //     currentUser.InstitucionId.Value,
        //     currentUser.UserId,
        //     currentUser.IpAddress,
        //     JsonSerializer.Serialize(new { ReservaId = reservaId }),
        //     reservaId);

        return Ok();
    }
}

// =============================================
// PATRONES COMUNES DE CORRECCIÓN
// =============================================

/*
1. CAMBIAR ESTO:
   ❌ var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
   ❌ if (!int.TryParse(userIdClaim, out int userId))

   POR ESTO:
   ✅ var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
   ✅ if (string.IsNullOrEmpty(userId))

2. CAMBIAR FIRMAS DE MÉTODOS:
   ❌ ComprehensiveCancelOccupationAsync(..., int userId, ...)
   ✅ ComprehensiveCancelOccupationAsync(..., string userId, ...)

3. CAMBIAR LOGS:
   ❌ _logger.LogInformation("... by user {UserId}", userId); // int
   ✅ _logger.LogInformation("... by user {UserId}", userId); // string

4. USAR EXTENSION METHODS:
   ✅ var userId = this.GetCurrentUserId();
   ✅ var userInfo = this.GetCurrentUserInfo();
   ✅ var institucionId = this.GetCurrentInstitucionId();
   ✅ var ipAddress = this.GetClientIpAddress();
*/