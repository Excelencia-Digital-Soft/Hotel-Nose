namespace hotel.DTOs.Inventory;

public class StockValidationRequestDto
{
    public int ArticuloId { get; set; }
    public int RequestedQuantity { get; set; }
    public InventoryLocationType LocationType { get; set; }
    public int? LocationId { get; set; }
}