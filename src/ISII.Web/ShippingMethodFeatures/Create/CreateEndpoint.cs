using FastEndpoints;
using FluentValidation;
using ISII.Web.Domain.ShippingMethodAggregate;
using ISII.Web.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ISII.Web.ShippingMethodFeatures.Create;

public sealed class CreateShippingMethodRequest
{
  public string Name { get; init; } = string.Empty;
  public decimal Cost { get; init; }
  public int EstimatedDeliveryDays { get; init; }
}

public sealed class CreateShippingMethodValidator : Validator<CreateShippingMethodRequest>
{
  public CreateShippingMethodValidator()
  {
    RuleFor(x => x.Name).NotEmpty().WithMessage("El nombre es obligatorio.")
      .Must(name => name is not null && name.Trim().Length <= ShippingMethod.MaxNameLength)
      .WithMessage("El nombre debe tener hasta 100 caracteres.");
    RuleFor(x => x.Cost).GreaterThanOrEqualTo(0).WithMessage("El costo no puede ser negativo.")
      .Must(cost => decimal.Round(cost, 2) == cost).WithMessage("El costo admite hasta dos decimales.");
    RuleFor(x => x.EstimatedDeliveryDays).InclusiveBetween(1, ShippingMethod.MaxDeliveryDays)
      .WithMessage("El plazo debe estar entre 1 y 60 días.");
  }
}

public class CreateEndpoint(IMediator mediator)
  : Endpoint<CreateShippingMethodRequest, Results<Created<ShippingMethodRecord>, ValidationProblem, ProblemHttpResult>>
{
  public override void Configure()
  {
    Post("/ShippingMethods");
    AllowAnonymous();
    Tags("ShippingMethods");
    Summary(s =>
    {
      s.Summary = "Crear un método de envío";
      s.Description = "Registra nombre, costo y plazo estimado en días. Admite envío gratuito.";
      s.ExampleRequest = new CreateShippingMethodRequest { Name = "Envío local", Cost = 2500m, EstimatedDeliveryDays = 2 };
      s.Responses[201] = "Método creado; Location contiene la URL para consultarlo.";
      s.Responses[400] = "Datos inválidos.";
    });
  }

  public override async Task<Results<Created<ShippingMethodRecord>, ValidationProblem, ProblemHttpResult>>
    ExecuteAsync(CreateShippingMethodRequest request, CancellationToken ct)
  {
    var result = await mediator.Send(new CreateShippingMethodCommand(request.Name, request.Cost, request.EstimatedDeliveryDays), ct);
    return result.ToCreatedResult(value => $"/ShippingMethods/{value.Id}", value => value);
  }
}
