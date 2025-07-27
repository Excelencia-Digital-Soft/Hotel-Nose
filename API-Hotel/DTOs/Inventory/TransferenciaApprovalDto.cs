namespace hotel.DTOs.Inventory;

/// <summary>
/// DTO for transfer approval/rejection
/// </summary>
public class TransferenciaApprovalDto
{
    /// <summary>
    /// Indicates if the transfer is approved
    /// </summary>
    public bool Approved { get; set; }

    /// <summary>
    /// Approval or rejection comments
    /// </summary>
    public string? Comments { get; set; }
}