using System;

namespace Hardware.domain.Entities
{
    public class StoreTerms
    {
        public int TermsId { get; set; }
        public string ReturnPolicy { get; set; } = "Items can be returned within 7 days with valid official receipt.";
        public string CreditRules { get; set; } = "Store credit applies for approved returns in good physical condition.";
        public string GeneralTerms { get; set; } = "All sales final after 7 days. Guarantee voids if tampered.";
    }
}
