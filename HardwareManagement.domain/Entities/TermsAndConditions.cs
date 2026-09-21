using System;

namespace Hardware.domain.Entities;

public class TermsAndConditions
{
    public int TermsAndConditionsId { get; set; }
    public int CompanyId { get; set; }
    public string StoreRules { get; set; } = "Default Store Policy: Items can be returned within 7 days with original receipt.";
    public string ReturnPolicy { get; set; } = "7 days return / replacement policy for factory defects.";
    public string WarrantyTerms { get; set; } = "6 months standard manufacturer warranty.";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
