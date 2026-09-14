using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Hardware.infrastructure.Data
{
    public class TenantErpDbContextFactory : IDesignTimeDbContextFactory<TenantErpDbContext>
    {
        public TenantErpDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TenantErpDbContext>();

            // Hardcode your MonsterASP connection string for design-time migration generation
            optionsBuilder.UseSqlServer("Server=db66917.public.databaseasp.net; Database=db66917; User Id=db66917; Password=jW@6?2Yq9Ho%; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;");

            return new TenantErpDbContext(optionsBuilder.Options);
        }
    }
}