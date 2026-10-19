namespace ISII.Web.Domain.ShippingMethodAggregate.Specifications;

public sealed class ShippingMethodByIdSpec : Specification<ShippingMethod>, ISingleResultSpecification<ShippingMethod>
{
  public ShippingMethodByIdSpec(ShippingMethodId id) => Query.Where(entity => entity.Id == id);
}
