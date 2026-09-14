using System;
using System.Collections.Generic;
using System.Text;

using Hardware.infrastructure.Data;

namespace Hardware.infrastructure.Services
{
    public interface ITenantDbContextFactory
    {
        Task<TenantErpDbContext> CreateAsync(int companyId);
    }
}
