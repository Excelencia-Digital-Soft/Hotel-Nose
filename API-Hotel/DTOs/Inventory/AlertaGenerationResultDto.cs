namespace hotel.DTOs.Inventory;

/// <summary>
/// Result DTO for alert generation process
/// </summary>
public class AlertaGenerationResultDto
{
    /// <summary>
    /// Inventory ID processed
    /// </summary>
    public int InventoryId { get; set; }

    /// <summary>
    /// Alerts generated in this process
    /// </summary>
    public List<AlertaInventarioDto> AlertasGeneradas { get; set; } = new();

    /// <summary>
    /// Alert IDs that were deactivated
    /// </summary>
    public List<int> AlertasDesactivadas { get; set; } = new();

    /// <summary>
    /// Summary message
    /// </summary>
    public string? Mensaje { get; set; }

    /// <summary>
    /// Indicates if the process was successful
    /// </summary>
    public bool Success => !Errors.Any();

    /// <summary>
    /// List of errors during generation
    /// </summary>
    public List<string> Errors { get; set; } = new();
}