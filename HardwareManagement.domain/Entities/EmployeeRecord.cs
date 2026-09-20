using System;

namespace Hardware.domain.Entities
{
    public class EmployeeRecord
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public decimal BaseSalary { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
