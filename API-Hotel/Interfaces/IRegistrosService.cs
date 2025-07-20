using hotel.DTOs.Common;
using hotel.DTOs.Registros;
using hotel.Models;

namespace hotel.Interfaces;

/// <summary>
/// Servicio para gestión de registros del sistema con capacidades de auditoría y logging
/// </summary>
public interface IRegistrosService
{
    /// <summary>
    /// Obtiene todos los registros de una institución con filtros opcionales
    /// </summary>
    /// <param name="institucionId">ID de la institución</param>
    /// <param name="tipoRegistro">Filtro opcional por tipo de registro</param>
    /// <param name="modulo">Filtro opcional por módulo</param>
    /// <param name="fechaDesde">Filtro opcional fecha desde</param>
    /// <param name="fechaHasta">Filtro opcional fecha hasta</param>
    /// <param name="usuarioId">Filtro opcional por usuario</param>
    /// <param name="pageNumber">Número de página para paginación</param>
    /// <param name="pageSize">Tamaño de página para paginación</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista paginada de registros</returns>
    Task<ApiResponse<RegistrosPaginadosDto>> GetRegistrosAsync(
        int institucionId,
        TipoRegistro? tipoRegistro = null,
        ModuloSistema? modulo = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        string? usuarioId = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un registro específico por ID
    /// </summary>
    /// <param name="registroId">ID del registro</param>
    /// <param name="institucionId">ID de la institución</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Registro encontrado</returns>
    Task<ApiResponse<RegistroDto>> GetRegistroByIdAsync(
        int registroId,
        int institucionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea un nuevo registro en el sistema
    /// </summary>
    /// <param name="createDto">Datos del registro a crear</param>
    /// <param name="institucionId">ID de la institución</param>
    /// <param name="usuarioId">ID del usuario que crea el registro</param>
    /// <param name="direccionIP">Dirección IP del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Registro creado</returns>
    Task<ApiResponse<RegistroDto>> CreateRegistroAsync(
        RegistroCreateDto createDto,
        int institucionId,
        string? usuarioId = null,
        string? direccionIP = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Método conveniente para crear registros de información
    /// </summary>
    /// <param name="contenido">Contenido del registro</param>
    /// <param name="modulo">Módulo que genera el registro</param>
    /// <param name="institucionId">ID de la institución</param>
    /// <param name="usuarioId">ID del usuario</param>
    /// <param name="direccionIP">Dirección IP</param>
    /// <param name="detallesAdicionales">Detalles adicionales en JSON</param>
    /// <param name="reservaId">ID de reserva relacionada (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Registro creado</returns>
    Task<ApiResponse<RegistroDto>> LogInfoAsync(
        string contenido,
        ModuloSistema modulo,
        int institucionId,
        string? usuarioId = null,
        string? direccionIP = null,
        string? detallesAdicionales = null,
        int? reservaId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Método conveniente para crear registros de advertencia
    /// </summary>
    /// <param name="contenido">Contenido del registro</param>
    /// <param name="modulo">Módulo que genera el registro</param>
    /// <param name="institucionId">ID de la institución</param>
    /// <param name="usuarioId">ID del usuario</param>
    /// <param name="direccionIP">Dirección IP</param>
    /// <param name="detallesAdicionales">Detalles adicionales en JSON</param>
    /// <param name="reservaId">ID de reserva relacionada (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Registro creado</returns>
    Task<ApiResponse<RegistroDto>> LogWarningAsync(
        string contenido,
        ModuloSistema modulo,
        int institucionId,
        string? usuarioId = null,
        string? direccionIP = null,
        string? detallesAdicionales = null,
        int? reservaId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Método conveniente para crear registros de error
    /// </summary>
    /// <param name="contenido">Contenido del registro</param>
    /// <param name="modulo">Módulo que genera el registro</param>
    /// <param name="institucionId">ID de la institución</param>
    /// <param name="usuarioId">ID del usuario</param>
    /// <param name="direccionIP">Dirección IP</param>
    /// <param name="detallesAdicionales">Detalles adicionales en JSON</param>
    /// <param name="reservaId">ID de reserva relacionada (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Registro creado</returns>
    Task<ApiResponse<RegistroDto>> LogErrorAsync(
        string contenido,
        ModuloSistema modulo,
        int institucionId,
        string? usuarioId = null,
        string? direccionIP = null,
        string? detallesAdicionales = null,
        int? reservaId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Método conveniente para crear registros de auditoría
    /// </summary>
    /// <param name="contenido">Contenido del registro</param>
    /// <param name="modulo">Módulo que genera el registro</param>
    /// <param name="institucionId">ID de la institución</param>
    /// <param name="usuarioId">ID del usuario</param>
    /// <param name="direccionIP">Dirección IP</param>
    /// <param name="detallesAdicionales">Detalles adicionales en JSON</param>
    /// <param name="reservaId">ID de reserva relacionada (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Registro creado</returns>
    Task<ApiResponse<RegistroDto>> LogAuditAsync(
        string contenido,
        ModuloSistema modulo,
        int institucionId,
        string? usuarioId = null,
        string? direccionIP = null,
        string? detallesAdicionales = null,
        int? reservaId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Método conveniente para crear registros de seguridad
    /// </summary>
    /// <param name="contenido">Contenido del registro</param>
    /// <param name="modulo">Módulo que genera el registro</param>
    /// <param name="institucionId">ID de la institución</param>
    /// <param name="usuarioId">ID del usuario</param>
    /// <param name="direccionIP">Dirección IP</param>
    /// <param name="detallesAdicionales">Detalles adicionales en JSON</param>
    /// <param name="reservaId">ID de reserva relacionada (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Registro creado</returns>
    Task<ApiResponse<RegistroDto>> LogSecurityAsync(
        string contenido,
        ModuloSistema modulo,
        int institucionId,
        string? usuarioId = null,
        string? direccionIP = null,
        string? detallesAdicionales = null,
        int? reservaId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina registros antiguos según política de retención
    /// </summary>
    /// <param name="institucionId">ID de la institución</param>
    /// <param name="diasRetencion">Días de retención (por defecto 90 días)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de registros eliminados</returns>
    Task<ApiResponse<int>> LimpiarRegistrosAntiguosAsync(
        int institucionId,
        int diasRetencion = 90,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Realiza soft delete de un registro
    /// </summary>
    /// <param name="registroId">ID del registro</param>
    /// <param name="institucionId">ID de la institución</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la operación</returns>
    Task<ApiResponse> AnularRegistroAsync(
        int registroId,
        int institucionId,
        CancellationToken cancellationToken = default);
}