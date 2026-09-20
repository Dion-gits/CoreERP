using System;
using System.Collections.Generic;

namespace Hardware.domain.Entities;

public class PurchaseOrder
{
    public int PurchaseOrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Validated, Funds Approved, Received, Cancelled
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}

public class PurchaseOrderItem
{
    public int PurchaseOrderItemId { get; set; }
    public int PurchaseOrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal SubTotal { get; set; }
}
