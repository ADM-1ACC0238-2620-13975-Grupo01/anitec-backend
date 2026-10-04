using Anitec.Platform.Livestock.Application.Internal.CommandServices;
using Anitec.Platform.Livestock.Domain.Model;
using Anitec.Platform.Livestock.Domain.Model.Commands;
using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Livestock.Domain.Repositories;
using Anitec.Platform.Shared.Domain.Repositories;
using NSubstitute;

namespace Anitec.Platform.Tests.Unit.Livestock;

public class CorralCommandServiceTests
{
    private readonly ICorralRepository _corrals = Substitute.For<ICorralRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CorralCommandService _service;

    public CorralCommandServiceTests()
    {
        _service = new CorralCommandService(_corrals, _unitOfWork);
    }

    [Fact]
    public async Task Create_SavesTheCorralInItsFarm()
    {
        Corral? saved = null;
        await _corrals.AddAsync(Arg.Do<Corral>(c => saved = c), Arg.Any<CancellationToken>());

        var result = await _service.Handle(new CreateCorralCommand("Corral A", 7), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Corral A", saved!.Name);
        Assert.Equal(7, saved.HerdId);
        await _unitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_RenamesAndMovesTheCorral()
    {
        var existing = new Corral { Id = 3, Name = "Old", HerdId = 1 };
        _corrals.FindByIdAsync(3, Arg.Any<CancellationToken>()).Returns(existing);

        var result = await _service.Handle(new UpdateCorralCommand(3, "New", 2), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("New", existing.Name);
        Assert.Equal(2, existing.HerdId);
        _corrals.Received(1).Update(existing);
    }

    [Fact]
    public async Task Update_OfAMissingCorral_FailsWithCorralNotFound()
    {
        _corrals.FindByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Corral?)null);

        var result = await _service.Handle(new UpdateCorralCommand(99, "New", 2), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(LivestockError.CorralNotFound, result.Error);
        await _unitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_RemovesTheCorral()
    {
        var existing = new Corral { Id = 3, Name = "Old" };
        _corrals.FindByIdAsync(3, Arg.Any<CancellationToken>()).Returns(existing);

        var result = await _service.Handle(new DeleteCorralCommand(3), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _corrals.Received(1).Remove(existing);
        await _unitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_OfAMissingCorral_FailsWithCorralNotFound()
    {
        _corrals.FindByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Corral?)null);

        var result = await _service.Handle(new DeleteCorralCommand(99), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(LivestockError.CorralNotFound, result.Error);
    }
}
