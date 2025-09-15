namespace hotel.DTOs.Inventory;

/// <summary>
/// DTO for transfer execution
/// </summary>
public class TransferenciaExecutionDto
{
    /// <summary>
    /// Notes about the execution
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Allow partial completion if some items fail
    /// </summary>
    public bool AllowPartialCompletion { get; set; } = true;
}