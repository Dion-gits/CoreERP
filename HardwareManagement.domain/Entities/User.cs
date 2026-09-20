using System;

namespace Hardware.domain.Entities;

public class User
{
    public int UserId { get; set; }
    public int CompanyId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Password { get; set; } = "123123";
    public bool IsActive { get; set; } = true;
}
