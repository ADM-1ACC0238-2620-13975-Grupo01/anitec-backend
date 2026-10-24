using Anitec.Platform.Clients.Domain.Model.Entities;
using Anitec.Platform.Iam.Application.Internal.OutboundServices;
using Anitec.Platform.Iam.Domain.Model.Aggregates;
using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Anitec.Platform.Sanitary.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Anitec.Platform.Tests.Integration;

/// <summary>The development seed is the data every developer sees first, so its shape is checked.</summary>
public class SeedDataTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private async Task<AppDbContext> SeededAsync()
    {
        var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await AppDbContextSeeder.SeedDevelopmentDataAsync(context, scope.ServiceProvider.GetRequiredService<IHashingService>());
        return context;
    }

    [Fact]
    public async Task EveryAnimal_HasACorralOfItsOwnFarm()
    {
        var context = await SeededAsync();

        var animals = await context.Set<Animal>().ToListAsync();
        var corrals = await context.Set<Corral>().ToDictionaryAsync(corral => corral.Id);

        Assert.True(animals.Count >= 150, $"Expected plenty of animals, found {animals.Count}");
        Assert.All(animals, animal =>
        {
            Assert.NotNull(animal.CorralId);
            Assert.Equal(animal.HerdId, corrals[animal.CorralId!.Value].HerdId);
        });
        Assert.Equal(animals.Count, animals.Select(animal => animal.Tag).Distinct().Count());
    }

    [Fact]
    public async Task ARancher_NeverHasTwoAnimalsWithTheSameName()
    {
        var context = await SeededAsync();
        var herdOwner = await context.Set<Herd>().ToDictionaryAsync(herd => herd.Id, herd => herd.OwnerId);

        var repeated = (await context.Set<Animal>().ToListAsync())
            .GroupBy(animal => (Owner: herdOwner[animal.HerdId], animal.Name))
            .Where(group => group.Count() > 1)
            .Select(group => group.Key.Name)
            .ToList();

        Assert.Empty(repeated);
    }

    [Fact]
    public async Task ThreeRanchers_HaveSeveralFarmsManyCorralsAndManyAnimals()
    {
        var context = await SeededAsync();
        var herds = await context.Set<Herd>().ToListAsync();
        var corrals = await context.Set<Corral>().ToListAsync();
        var animals = await context.Set<Animal>().ToListAsync();

        foreach (var username in new[] { "ganadero", "maria", "jose" })
        {
            var rancher = await context.Set<User>().SingleAsync(user => user.Username == username);
            var farmIds = herds.Where(herd => herd.OwnerId == rancher.Id).Select(herd => herd.Id).ToHashSet();

            Assert.True(farmIds.Count >= 3, $"{username} has {farmIds.Count} farms");
            Assert.True(corrals.Count(corral => farmIds.Contains(corral.HerdId)) >= 9, $"{username} needs several corrals");
            Assert.True(animals.Count(animal => farmIds.Contains(animal.HerdId)) >= 40, $"{username} needs many animals");
        }
    }

    [Fact]
    public async Task TheVeterinarian_HasTheThreeRanchersAsClients()
    {
        var context = await SeededAsync();
        var veterinarian = await context.Set<User>().SingleAsync(user => user.Username == "veterinaria");
        var ranchers = await context.Set<User>().Where(user => new[] { "ganadero", "maria", "jose" }.Contains(user.Username)).ToListAsync();

        var clientIds = (await context.Set<VeterinarianClient>().Where(client => client.VeterinarianId == veterinarian.Id).ToListAsync())
            .Select(client => client.RancherId).ToHashSet();

        Assert.Equal(3, ranchers.Count);
        Assert.All(ranchers, rancher => Assert.Contains(rancher.Id, clientIds));
    }

    [Fact]
    public async Task TheSeed_IncludesArchivedAnimals_PoultryNamedByNumber_AndUpcomingFollowUps()
    {
        var context = await SeededAsync();
        var animals = await context.Set<Animal>().ToListAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        Assert.Contains(animals, animal => animal.Status == Animal.ArchivedStatus);
        Assert.Contains(animals, animal => animal.Name == "polloComercio-001");
        Assert.Contains(animals, animal => animal.Name == "polloComercio-010");
        Assert.Contains(await context.Set<HealthEvent>().ToListAsync(), healthEvent => healthEvent.NextDueDate > today);
    }
}
