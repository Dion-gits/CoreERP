using System;

namespace Hardware.domain.Entities;

public class PayrollRecord
{
    public int PayrollId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public decimal BaseSalary { get; set; }
    public decimal Bonuses { get; set; }
    public decimal Deductions { get; set; }
    public decimal NetPay { get; set; }
    public string PayPeriod { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Processed, Paid
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
