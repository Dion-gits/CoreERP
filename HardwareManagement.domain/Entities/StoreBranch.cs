using System;

namespace Hardware.domain.Entities
{
    public class StoreBranch
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
