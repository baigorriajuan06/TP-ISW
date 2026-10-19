using System.Net;
using System.Net.Http.Json;
using ISII.Web.Infrastructure.Data;
using ISII.Web.ShippingMethodFeatures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ISII.Tests;

public class ShippingMethodApiTests
{
  private sealed class ApiFactory(string databasePath) : WebApplicationFactory<Program>
  {
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      builder.UseEnvironment("Testing");
      builder.ConfigureServices(services =>
      {
        services.RemoveAll<DbContextOptions<AppDbContext>>();
        services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
        services.RemoveAll<AppDbContext>();
        services.AddDbContext<AppDbContext>(options => options.UseSqlite($"Data Source={databasePath};Pooling=False"));
      });
    }
  }

  [Fact]
  public async Task CreateReadAndRestartPreservesValues()
  {
    var path = Path.Combine(Path.GetTempPath(), $"shipping-{Guid.NewGuid()}.db");
    try
    {
      ShippingMethodRecord created;
      string location;
      using (var factory = new ApiFactory(path))
      using (var client = factory.CreateClient())
      {
        var post = await client.PostAsJsonAsync("/ShippingMethods", new { name = "  Envío local  ", cost = 2500.50m, estimatedDeliveryDays = 2 });
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);
        created = (await post.Content.ReadFromJsonAsync<ShippingMethodRecord>())!;
        location = post.Headers.Location!.ToString();
        Assert.Equal($"/ShippingMethods/{created.Id}", location);
        Assert.Equal("Envío local", created.Name);
        Assert.Equal(2500.50m, created.Cost);
        Assert.Equal(2, created.EstimatedDeliveryDays);
        Assert.Equal(created, await client.GetFromJsonAsync<ShippingMethodRecord>(location));
      }
      using (var restarted = new ApiFactory(path))
      using (var client = restarted.CreateClient())
        Assert.Equal(created, await client.GetFromJsonAsync<ShippingMethodRecord>(location));
    }
    finally { File.Delete(path); }
  }

  [Fact]
  public async Task ApiReturnsErrorsAndAcceptsFreeShipping()
  {
    var path = Path.Combine(Path.GetTempPath(), $"shipping-{Guid.NewGuid()}.db");
    try
    {
      using var factory = new ApiFactory(path);
      using var client = factory.CreateClient();
      foreach (var body in new[]
      {
        new { name = "", cost = 1m, estimatedDeliveryDays = 1 },
        new { name = "Local", cost = -1m, estimatedDeliveryDays = 1 },
        new { name = "Local", cost = 1.001m, estimatedDeliveryDays = 1 },
        new { name = "Local", cost = 1m, estimatedDeliveryDays = 0 }
      })
      {
        var response = await client.PostAsJsonAsync("/ShippingMethods", body);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }
      Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/ShippingMethods/not-a-guid")).StatusCode);
      Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync($"/ShippingMethods/{Guid.Empty}")).StatusCode);
      Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/ShippingMethods/{Guid.NewGuid()}")).StatusCode);
      Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/ShippingMethods",
        new { name = "Envío gratuito", cost = 0m, estimatedDeliveryDays = 1 })).StatusCode);
      using var scope = factory.Services.CreateScope();
      var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
      Assert.Equal(1, await db.ShippingMethods.CountAsync());
    }
    finally { File.Delete(path); }
  }
}
