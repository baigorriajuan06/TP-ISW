using ISII.Web.Domain.ShippingMethodAggregate;

namespace ISII.Web.ShippingMethodFeatures.Create;

public record CreateShippingMethodCommand(string Name, decimal Cost, int EstimatedDeliveryDays)
  : ICommand<Result<ShippingMethodRecord>>;

public class CreateShippingMethodHandler(IRepository<ShippingMethod> repository)
  : ICommandHandler<CreateShippingMethodCommand, Result<ShippingMethodRecord>>
{
  public async ValueTask<Result<ShippingMethodRecord>> Handle(CreateShippingMethodCommand request, CancellationToken ct)
  {
    ShippingMethod entity;
    try
    {
      entity = ShippingMethod.Create(request.Name, request.Cost, request.EstimatedDeliveryDays);
    }
    catch (ArgumentException ex)
    {
      return Result<ShippingMethodRecord>.Invalid(new ValidationError
      {
        Identifier = ex.ParamName ?? string.Empty,
        ErrorMessage = ex.Message
      });
    }

    // AddAsync del repositorio EF ya ejecuta SaveChangesAsync.
    await repository.AddAsync(entity, ct);
    return ShippingMethodRecord.FromEntity(entity);
  }
}
