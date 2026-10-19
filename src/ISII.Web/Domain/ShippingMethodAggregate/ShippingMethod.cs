namespace ISII.Web.Domain.ShippingMethodAggregate;

/// <summary>Opción de entrega ofrecida por el comercio, independiente de un pedido.</summary>
public class ShippingMethod : EntityBase<ShippingMethod, ShippingMethodId>, IAggregateRoot
{
  public const int MaxNameLength = 100;
  public const int MaxDeliveryDays = 60;
  private ShippingMethod() { } // EF Core

  public string Name { get; private set; } = string.Empty;
  public decimal Cost { get; private set; }
  public int EstimatedDeliveryDays { get; private set; }

  public static ShippingMethod Create(string name, decimal cost, int estimatedDeliveryDays)
  {
    if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > MaxNameLength)
      throw new ArgumentException("El nombre es obligatorio y debe tener hasta 100 caracteres.", nameof(name));
    if (cost < 0 || decimal.Round(cost, 2) != cost)
      throw new ArgumentOutOfRangeException(nameof(cost), "El costo debe ser mayor o igual a cero y tener hasta dos decimales.");
    if (estimatedDeliveryDays < 1 || estimatedDeliveryDays > MaxDeliveryDays)
      throw new ArgumentOutOfRangeException(nameof(estimatedDeliveryDays), "El plazo debe estar entre 1 y 60 días.");

    return new ShippingMethod
    {
      Id = ShippingMethodId.From(Guid.NewGuid()),
      Name = name.Trim(),
      Cost = cost,
      EstimatedDeliveryDays = estimatedDeliveryDays
    };
  }
}
