using Anitec.Platform.Livestock.Application.Internal.CommandServices;
using Anitec.Platform.Livestock.Domain.Model;
using Anitec.Platform.Livestock.Domain.Model.Commands;
using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Livestock.Domain.Repositories;
using Anitec.Platform.Shared.Domain.Repositories;
using NSubstitute;

namespace Anitec.Platform.Tests.Unit.Livestock;

public class AnimalCommandServiceTests
{
    private readonly IAnimalRepository _animals = Substitute.For<IAnimalRepository>();
    private readonly ICorralRepository _corrals = Substitute.For<ICorralRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly AnimalCommandService _service;

    public AnimalCommandServiceTests()
    {
        _service = new AnimalCommandService(_animals, _corrals, _unitOfWork);
    }

    private static CreateAnimalCommand NewAnimal(string tag = "COW-001") =>
        new(tag, "Lola", "Cattle", "Holstein", "Female", new DateOnly(2024, 3, 1), 420m, "Saludable", 1, 2, null,
            null, null);

    private static CreateAnimalBatchCommand NewBatch(int quantity, int corralId = 2) =>
        new("Cattle", "Holstein", "Female", null, 300m, "Saludable", 1, corralId, quantity, null, null, null);

    // ---------- Create ----------

    [Fact]
    public async Task Create_SavesTheAnimalWithTheCommandData()
    {
        Animal? saved = null;
        await _animals.AddAsync(Arg.Do<Animal>(a => saved = a), Arg.Any<CancellationToken>());

        var result = await _service.Handle(NewAnimal(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(saved, result.Value);
        Assert.Equal("COW-001", saved!.Tag);
        Assert.Equal("Lola", saved.Name);
        Assert.Equal(420m, saved.Weight);
        Assert.Equal(2, saved.CorralId);
        await _unitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    // ---------- Update ----------

    [Fact]
    public async Task Update_ChangesTheStoredAnimal()
    {
        var existing = new Animal(NewAnimal()) { Id = 5 };
        _animals.FindByIdAsync(5, Arg.Any<CancellationToken>()).Returns(existing);

        var result = await _service.Handle(
            new UpdateAnimalCommand(5, "COW-099", "Manchas", "Cattle", "Jersey", "Female", null, 450m, "En tratamiento",
                1, 3, null, null, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("COW-099", existing.Tag);
        Assert.Equal("En tratamiento", existing.Status);
        Assert.Equal(3, existing.CorralId);
        _animals.Received(1).Update(existing);
        await _unitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_OfAMissingAnimal_FailsWithAnimalNotFound()
    {
        _animals.FindByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Animal?)null);

        var result = await _service.Handle(
            new UpdateAnimalCommand(99, "T", "N", "S", "B", "G", null, 1m, "Saludable", 1, null, null, null, null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(LivestockError.AnimalNotFound, result.Error);
        await _unitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    // ---------- Delete ----------

    [Fact]
    public async Task Delete_RemovesTheAnimal()
    {
        var existing = new Animal(NewAnimal()) { Id = 5 };
        _animals.FindByIdAsync(5, Arg.Any<CancellationToken>()).Returns(existing);

        var result = await _service.Handle(new DeleteAnimalCommand(5), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _animals.Received(1).Remove(existing);
        await _unitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_OfAMissingAnimal_FailsWithAnimalNotFound()
    {
        _animals.FindByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Animal?)null);

        var result = await _service.Handle(new DeleteAnimalCommand(99), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(LivestockError.AnimalNotFound, result.Error);
    }

    // ---------- Batch creation ----------

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    [InlineData(501)]
    public async Task CreateBatch_WithAQuantityOutsideTheLimits_Fails(int quantity)
    {
        var result = await _service.Handle(NewBatch(quantity), CancellationToken.None);

        Assert.True(result.IsFailure);
        await _animals.DidNotReceive().AddAsync(Arg.Any<Animal>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(500)]
    public async Task CreateBatch_AcceptsTheLimitQuantities(int quantity)
    {
        _corrals.FindByIdAsync(2, Arg.Any<CancellationToken>()).Returns(new Corral { Id = 2, Name = "Corral A" });
        _animals.ListAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<Animal>());

        var result = await _service.Handle(NewBatch(quantity), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(quantity, result.Value!.Count);
    }

    [Fact]
    public async Task CreateBatch_WithAMissingCorral_FailsWithCorralNotFound()
    {
        _corrals.FindByIdAsync(2, Arg.Any<CancellationToken>()).Returns((Corral?)null);

        var result = await _service.Handle(NewBatch(3), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(LivestockError.CorralNotFound, result.Error);
    }

    [Fact]
    public async Task CreateBatch_NumbersTheTagsAfterTheAnimalsAlreadyInTheCorral()
    {
        _corrals.FindByIdAsync(2, Arg.Any<CancellationToken>()).Returns(new Corral { Id = 2, Name = "Corral A" });
        _animals.ListAsync(Arg.Any<CancellationToken>()).Returns(new[]
        {
            new Animal(NewAnimal("CorralA-001")),
            new Animal(NewAnimal("CorralA-002")),
            new Animal(NewAnimal("OTHER-001")) { CorralId = 9 }
        });

        var result = await _service.Handle(NewBatch(3), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(["CorralA-003", "CorralA-004", "CorralA-005"], result.Value!.Select(a => a.Tag));
        Assert.All(result.Value!, a => Assert.Equal(2, a.CorralId));
        await _unitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateBatch_UsesAGenericPrefixWhenTheCorralNameHasNoLettersOrDigits()
    {
        _corrals.FindByIdAsync(2, Arg.Any<CancellationToken>()).Returns(new Corral { Id = 2, Name = "---" });
        _animals.ListAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<Animal>());

        var result = await _service.Handle(NewBatch(1), CancellationToken.None);

        Assert.Equal("CORRAL-001", result.Value!.Single().Tag);
    }

    // ---------- Bulk status and bulk delete ----------

    [Fact]
    public async Task UpdateStatus_WithNoSelection_Fails()
    {
        var result = await _service.Handle(new UpdateAnimalsStatusCommand([], "Vendido"), CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task UpdateStatus_ChangesTheFoundAnimalsAndSkipsTheMissingOnes()
    {
        var first = new Animal(NewAnimal("A-1")) { Id = 1 };
        var third = new Animal(NewAnimal("A-3")) { Id = 3 };
        _animals.FindByIdAsync(1, Arg.Any<CancellationToken>()).Returns(first);
        _animals.FindByIdAsync(2, Arg.Any<CancellationToken>()).Returns((Animal?)null);
        _animals.FindByIdAsync(3, Arg.Any<CancellationToken>()).Returns(third);

        var result = await _service.Handle(new UpdateAnimalsStatusCommand([1, 2, 3], "Vendido"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Equal("Vendido", first.Status);
        Assert.Equal("Vendido", third.Status);
    }

    [Fact]
    public async Task DeleteMany_WithNoSelection_Fails()
    {
        var result = await _service.Handle(new DeleteAnimalsCommand([]), CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task DeleteMany_RemovesTheFoundAnimalsAndSkipsTheMissingOnes()
    {
        var first = new Animal(NewAnimal("A-1")) { Id = 1 };
        _animals.FindByIdAsync(1, Arg.Any<CancellationToken>()).Returns(first);
        _animals.FindByIdAsync(2, Arg.Any<CancellationToken>()).Returns((Animal?)null);

        var result = await _service.Handle(new DeleteAnimalsCommand([1, 2]), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _animals.Received(1).Remove(first);
        _animals.Received(1).Remove(Arg.Any<Animal>());
    }
}
