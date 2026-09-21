using System;

namespace Hardware.domain.Entities
{
    public class UnpaidExpense
    {
        public int ExpenseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = "Utility";
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(14);
        public bool IsPaid { get; set; } = false;
    }
}
