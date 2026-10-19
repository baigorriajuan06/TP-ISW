using Ardalis.Result;
using Ardalis.SharedKernel;
using ISII.Web.Domain.ShippingMethodAggregate;
using ISII.Web.Domain.ShippingMethodAggregate.Specifications;
using ISII.Web.ShippingMethodFeatures.Create;
using ISII.Web.ShippingMethodFeatures.GetById;
using NSubstitute;

namespace ISII.Tests;

public class ShippingMethodTests
{
  [Theory]
  [InlineData(0, 1)]
  [InlineData(1250.50, 60)]
  public void CreateAcceptsBoundariesAndTrimsName(decimal cost, int days)
  {
    var entity = ShippingMethod.Create("  Envío local  ", cost, days);
    Assert.NotEqual(Guid.Empty, entity.Id.Value);
    Assert.Equal("Envío local", entity.Name);
    Assert.Equal(cost, entity.Cost);
    Assert.Equal(days, entity.EstimatedDeliveryDays);
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  public void DomainRejectsMissingName(string? name)
    => Assert.Throws<ArgumentException>(() => ShippingMethod.Create(name!, 0, 1));

  [Fact]
  public void DomainRejectsLongName()
    => Assert.Throws<ArgumentException>(() => ShippingMethod.Create(new string('x', 101), 0, 1));

  [Fact]
  public void DomainAcceptsMaximumName()
    => Assert.Equal(100, ShippingMethod.Create(new string('x', 100), 0, 1).Name.Length);

  [Theory]
  [InlineData(-1, 1)]
  [InlineData(1.001, 1)]
  [InlineData(1, 0)]
  [InlineData(1, -1)]
  [InlineData(1, 61)]
  public void DomainRejectsInvalidCostOrDays(decimal cost, int days)
    => Assert.Throws<ArgumentOutOfRangeException>(() => ShippingMethod.Create("Local", cost, days));

  [Fact]
  public void IdentifiersAreGeneratedByTheDomain()
  {
    Assert.NotEqual(ShippingMethod.Create("Local", 0, 1).Id, ShippingMethod.Create("Local", 0, 1).Id);
    Assert.ThrowsAny<Exception>(() => ShippingMethodId.From(Guid.Empty));
  }

  [Theory]
  [InlineData("Local", 0, 1, true)]
  [InlineData("Local", 100.25, 60, true)]
  [InlineData(null, 0, 1, false)]
  [InlineData("", 0, 1, false)]
  [InlineData("   ", 0, 1, false)]
  [InlineData("Local", -1, 1, false)]
  [InlineData("Local", 0.001, 1, false)]
  [InlineData("Local", 0, 0, false)]
  [InlineData("Local", 0, 61, false)]
  public void RequestValidatorChecksInputs(string? name, decimal cost, int days, bool valid)
  {
    var validator = new CreateShippingMethodValidator();
    Assert.Equal(valid, validator.Validate(new CreateShippingMethodRequest
      { Name = name!, Cost = cost, EstimatedDeliveryDays = days }).IsValid);
  }

  [Fact]
  public void RequestValidatorRejectsLongName()
    => Assert.False(new CreateShippingMethodValidator().Validate(new CreateShippingMethodRequest
      { Name = new string('x', 101), Cost = 0, EstimatedDeliveryDays = 1 }).IsValid);

  [Fact]
  public void QueryValidatorRejectsEmptyGuid()
  {
    var validator = new GetShippingMethodByIdValidator();
    Assert.False(validator.Validate(new GetShippingMethodByIdRequest()).IsValid);
    Assert.True(validator.Validate(new GetShippingMethodByIdRequest { ShippingMethodId = Guid.NewGuid() }).IsValid);
  }

  [Fact]
  public async Task CreateHandlerPersistsAndReturnsObject()
  {
    var repository = Substitute.For<IRepository<ShippingMethod>>();
    var result = await new CreateShippingMethodHandler(repository)
      .Handle(new CreateShippingMethodCommand(" Local ", 100, 2), CancellationToken.None);
    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.Equal("Local", result.Value.Name);
    Assert.Equal(100, result.Value.Cost);
    Assert.Equal(2, result.Value.EstimatedDeliveryDays);
    await repository.Received(1).AddAsync(Arg.Is<ShippingMethod>(x => x.Id.Value == result.Value.Id), CancellationToken.None);
  }

  [Fact]
  public async Task CreateHandlerRejectsInvalidInputWithoutPersisting()
  {
    var repository = Substitute.For<IRepository<ShippingMethod>>();
    var result = await new CreateShippingMethodHandler(repository)
      .Handle(new CreateShippingMethodCommand("Local", -1, 2), CancellationToken.None);
    Assert.Equal(ResultStatus.Invalid, result.Status);
    await repository.DidNotReceive().AddAsync(Arg.Any<ShippingMethod>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task GetHandlerReturnsNotFound()
  {
    var repository = Substitute.For<IReadRepository<ShippingMethod>>();
    var result = await new GetShippingMethodHandler(repository)
      .Handle(new GetShippingMethodQuery(ShippingMethodId.From(Guid.NewGuid())), CancellationToken.None);
    Assert.Equal(ResultStatus.NotFound, result.Status);
  }

  [Fact]
  public async Task GetHandlerReturnsStoredValues()
  {
    var entity = ShippingMethod.Create("Local", 50.25m, 3);
    var repository = Substitute.For<IReadRepository<ShippingMethod>>();
    repository.FirstOrDefaultAsync(Arg.Any<ShippingMethodByIdSpec>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<ShippingMethod?>(entity));
    var result = await new GetShippingMethodHandler(repository)
      .Handle(new GetShippingMethodQuery(entity.Id), CancellationToken.None);
    Assert.Equal(ResultStatus.Ok, result.Status);
    Assert.Equal(entity.Id.Value, result.Value.Id);
    Assert.Equal(entity.Name, result.Value.Name);
    Assert.Equal(entity.Cost, result.Value.Cost);
    Assert.Equal(entity.EstimatedDeliveryDays, result.Value.EstimatedDeliveryDays);
  }
}
