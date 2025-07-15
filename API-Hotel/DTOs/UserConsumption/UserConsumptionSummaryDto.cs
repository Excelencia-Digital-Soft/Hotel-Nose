namespace hotel.DTOs.UserConsumption;

public class UserConsumptionSummaryDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int TotalItems { get; set; }
    public int TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
    public Dictionary<string, decimal> AmountByType { get; set; } = new();
    public List<TopConsumedItem> TopConsumedItems { get; set; } = new();
}

public class TopConsumedItem
{
    public int ArticuloId { get; set; }
    public string ArticuloNombre { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
}