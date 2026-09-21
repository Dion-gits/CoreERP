using System;

namespace Hardware.domain.Entities
{
    public class StockAuditRequest
    {
        public int AuditRequestId { get; set; }
        public int ProductId { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public decimal SystemQty { get; set; }
        public decimal PhysicalQty { get; set; }
        public decimal VarianceQty { get; set; }
        public string Status { get; set; } = "Pending";
        public string ValidatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Product? Product { get; set; }
    }
}
