using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ISII.Web.Infrastructure.Data;

/// <summary>Usa el mismo proveedor SQLite que la aplicación al generar migraciones.</summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
  public AppDbContext CreateDbContext(string[] args)
  {
    var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__AppDb")
      ?? "DataSource=ISII.db;Cache=Shared";
    return new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connectionString).Options);
  }
}
