using ISII.Web.Domain.ShippingMethodAggregate;
using ISII.Web.Domain.ShippingMethodAggregate.Specifications;

namespace ISII.Web.ShippingMethodFeatures.GetById;

public record GetShippingMethodQuery(ShippingMethodId Id) : IQuery<Result<ShippingMethodRecord>>;

public class GetShippingMethodHandler(IReadRepository<ShippingMethod> repository)
  : IQueryHandler<GetShippingMethodQuery, Result<ShippingMethodRecord>>
{
  public async ValueTask<Result<ShippingMethodRecord>> Handle(GetShippingMethodQuery request, CancellationToken ct)
  {
    var entity = await repository.FirstOrDefaultAsync(new ShippingMethodByIdSpec(request.Id), ct);
    return entity is null ? Result<ShippingMethodRecord>.NotFound() : ShippingMethodRecord.FromEntity(entity);
  }
}
