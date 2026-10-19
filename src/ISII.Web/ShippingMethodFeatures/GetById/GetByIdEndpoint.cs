using FastEndpoints;
using FluentValidation;
using ISII.Web.Domain.ShippingMethodAggregate;
using ISII.Web.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ISII.Web.ShippingMethodFeatures.GetById;

public sealed class GetShippingMethodByIdRequest
{
  public Guid ShippingMethodId { get; init; }
}

public sealed class GetShippingMethodByIdValidator : Validator<GetShippingMethodByIdRequest>
{
  public GetShippingMethodByIdValidator()
  {
    RuleFor(x => x.ShippingMethodId).NotEmpty().WithMessage("El identificador debe ser un GUID válido y no vacío.");
  }
}

public class GetByIdEndpoint(IMediator mediator)
  : Endpoint<GetShippingMethodByIdRequest, Results<Ok<ShippingMethodRecord>, NotFound, ProblemHttpResult>>
{
  public override void Configure()
  {
    Get("/ShippingMethods/{ShippingMethodId}");
    AllowAnonymous();
    Tags("ShippingMethods");
    Summary(s =>
    {
      s.Summary = "Consultar un método de envío por identificador";
      s.Responses[200] = "Método encontrado.";
      s.Responses[400] = "Identificador inválido.";
      s.Responses[404] = "No existe un método con ese identificador.";
    });
  }

  public override async Task<Results<Ok<ShippingMethodRecord>, NotFound, ProblemHttpResult>>
    ExecuteAsync(GetShippingMethodByIdRequest request, CancellationToken ct)
  {
    var result = await mediator.Send(new GetShippingMethodQuery(ShippingMethodId.From(request.ShippingMethodId)), ct);
    return result.ToGetByIdResult(value => value);
  }
}
