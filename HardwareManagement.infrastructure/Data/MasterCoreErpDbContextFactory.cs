using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Hardware.infrastructure.Data
{
    public class MasterCoreErpDbContextFactory : IDesignTimeDbContextFactory<MasterCoreErpDbContext>
    {
        public MasterCoreErpDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MasterCoreErpDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=db66917.public.databaseasp.net; Database=db66917; User Id=db66917; Password=jW@6?2Yq9Ho%; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;",
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });

            return new MasterCoreErpDbContext(optionsBuilder.Options);
        }
    }
}
