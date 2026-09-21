using System;

namespace Hardware.domain.Entities;

public class StockBox
{
    public int StockBoxId { get; set; }
    public int ProductId { get; set; }
    public string BoxNumber { get; set; } = string.Empty;
    public decimal UnitsPerBox { get; set; }
    public bool IsConverted { get; set; } = false;
    public DateTime? ConvertedAt { get; set; }

    public Product? Product { get; set; }
}
