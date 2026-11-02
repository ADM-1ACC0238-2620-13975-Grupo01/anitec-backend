using Anitec.Platform.Subscriptions.Domain.Model.Commands;

namespace Anitec.Platform.Subscriptions.Domain.Model.Entities;

/// <summary>
/// A paid plan users can subscribe to, linked to a Stripe price.
/// </summary>
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
    /// <summary>Id of the matching price in Stripe.</summary>
    public string StripePriceId { get; set; }
    /// <summary>Maximum number of animals allowed by the plan.</summary>
    public int MaxAnimals { get; set; }
    /// <summary>Whether the plan can still be chosen.</summary>
    public bool IsActive { get; set; }
}
