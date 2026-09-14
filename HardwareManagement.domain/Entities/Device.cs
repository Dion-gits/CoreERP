using System;
using System.Collections.Generic;
using System.Text;

namespace Hardware.domain.Entities;

public class Device
{
    public int DeviceId { get; set; }
    public int CompanyId { get; set; }
    public string DeviceCode { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property pointing back to Company
    public Company? Company { get; set; }
}
