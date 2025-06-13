using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ErpPortal.Infrastructure.Data
{
    public class ErpPortalDbContextFactory : IDesignTimeDbContextFactory<ErpPortalDbContext>
    {
        public ErpPortalDbContext CreateDbContext(string[] args)
        {
            // Build configuration
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../ErpPortal.Web"))
                .AddJsonFile("appsettings.json")
                .Build();

            // Get connection string
            var builder = new DbContextOptionsBuilder<ErpPortalDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            builder.UseSqlServer(connectionString);

            return new ErpPortalDbContext(builder.Options);
        }
    }
} 