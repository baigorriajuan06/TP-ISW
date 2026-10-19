using Vogen;

namespace ISII.Web.Domain.ShippingMethodAggregate;

[ValueObject<Guid>]
public readonly partial struct ShippingMethodId
{
  private static Validation Validate(Guid value)
    => value != Guid.Empty ? Validation.Ok : Validation.Invalid("ShippingMethodId cannot be empty.");
}
