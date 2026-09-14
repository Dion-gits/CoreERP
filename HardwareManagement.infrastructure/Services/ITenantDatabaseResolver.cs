using System;
using System.Collections.Generic;
using System.Text;

namespace Hardware.infrastructure.Services
{
    public interface ITenantDatabaseResolver
    {
        Task<TenantDatabaseInfo> GetDatabaseInfoAsync(int companyId);
    }
}
