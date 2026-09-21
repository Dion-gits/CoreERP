using System;

namespace Hardware.domain.Entities
{
    public class StockTransfer
    {
        public int TransferId { get; set; }
        public string TransferNumber { get; set; } = string.Empty;
        public int SourceBranchId { get; set; }
        public int DestBranchId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public string Status { get; set; } = "Requested";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Product? Product { get; set; }
    }
}
