using ISII.Web.Domain.ShippingMethodAggregate;

namespace ISII.Web.ShippingMethodFeatures;

public record ShippingMethodRecord(Guid Id, string Name, decimal Cost, int EstimatedDeliveryDays)
{
  public static ShippingMethodRecord FromEntity(ShippingMethod entity)
    => new(entity.Id.Value, entity.Name, entity.Cost, entity.EstimatedDeliveryDays);
}
