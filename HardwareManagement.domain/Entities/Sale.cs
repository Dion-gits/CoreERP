using System;
using System.Collections.Generic;
using System.Net.ServerSentEvents;
using System.Text;

namespace Hardware.domain.Entities
{
    public class Sale
    {
        public int SaleId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime SaleDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public Customer? Customer { get; set; }
        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}
