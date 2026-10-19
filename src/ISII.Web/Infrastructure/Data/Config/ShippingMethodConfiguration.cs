using ISII.Web.Domain.ShippingMethodAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ISII.Web.Infrastructure.Data.Config;

public class ShippingMethodConfiguration : IEntityTypeConfiguration<ShippingMethod>
{
  public void Configure(EntityTypeBuilder<ShippingMethod> builder)
  {
    builder.Property(entity => entity.Id).HasVogenConversion().ValueGeneratedNever().IsRequired();
    builder.Property(entity => entity.Name).HasMaxLength(ShippingMethod.MaxNameLength).IsRequired();
    builder.Property(entity => entity.Cost).HasPrecision(18, 2).IsRequired();
    builder.Property(entity => entity.EstimatedDeliveryDays).IsRequired();
  }
}
