using Anitec.Platform.Subscriptions.Domain.Model.Commands;

namespace Anitec.Platform.Subscriptions.Domain.Model.Entities;

public class SubscriptionPlan
{
    public SubscriptionPlan()
    {
        Name = string.Empty;
        StripePriceId = string.Empty;
    }

    public SubscriptionPlan(CreateSubscriptionPlanCommand command)
    {
        Name = command.Name;
        Price = command.Price;
        StripePriceId = command.StripePriceId;
        MaxAnimals = command.MaxAnimals;
        IsActive = command.IsActive;

    }

    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string StripePriceId { get; set; }
    public int MaxAnimals { get; set; }
    public bool IsActive { get; set; }
}
