using System;

namespace Hardware.domain.Entities;

public class StockAuditRequest
{
    public int StockAuditRequestId { get; set; }
    public int ProductId { get; set; }
    public decimal PhysicalCount { get; set; }
    public decimal SystemCount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending Validation"; // Pending Validation, Validated, Rejected
    public string RequestedBy { get; set; } = string.Empty;
    public string? ValidatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Product? Product { get; set; }
}
