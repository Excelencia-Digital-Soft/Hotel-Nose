namespace hotel.DTOs.UserConsumption;

public class UserConsumptionByServiceDto
{
    public string ServiceType { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Percentage { get; set; }
    public List<ServiceItemDetail> Items { get; set; } = new();
}

public class ServiceItemDetail
{
    public int ArticuloId { get; set; }
    public string ArticuloNombre { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Amount { get; set; }
    public DateTime LastConsumedDate { get; set; }
}