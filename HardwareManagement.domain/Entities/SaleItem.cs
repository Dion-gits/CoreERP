using System;
using System.Collections.Generic;
using System.Text;

namespace Hardware.domain.Entities
{
    public class SaleItem
    {
        public int SaleItemId { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }

        public Sale? Sale { get; set; }
        public Product? Product { get; set; }
    }
}
