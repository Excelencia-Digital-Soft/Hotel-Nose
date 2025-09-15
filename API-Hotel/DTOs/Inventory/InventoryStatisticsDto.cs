namespace hotel.DTOs.Inventory;

public class InventoryStatisticsDto
{
    public int TotalItems { get; set; }
    public decimal TotalValue { get; set; }
    public int UniqueArticles { get; set; }
    public int LowStockItems { get; set; }
    public int ZeroStockItems { get; set; }
    public List<LocationStatisticsDto> LocationBreakdown { get; set; } = new();
    public DateTime GeneratedDate { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
}

public class LocationStatisticsDto
{
    public InventoryLocationType LocationType { get; set; }
    public int TotalItems { get; set; }
    public decimal TotalValue { get; set; }
    public int UniqueArticles { get; set; }
}