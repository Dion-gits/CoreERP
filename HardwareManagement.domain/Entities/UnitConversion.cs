using System;

namespace Hardware.domain.Entities
{
    public class UnitConversion
    {
        public int ConversionId { get; set; }
        public int ProductId { get; set; }
        public string UnitName { get; set; } = "Box";
        public decimal FactorToUnits { get; set; } = 20m;
        public decimal BoxesInStock { get; set; } = 0m;

        public Product? Product { get; set; }
    }
}
