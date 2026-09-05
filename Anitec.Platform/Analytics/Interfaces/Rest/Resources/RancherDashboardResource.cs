namespace Anitec.Platform.Analytics.Interfaces.Rest.Resources;

public record RancherDashboardResource(
    int Herds,
    int Animals,
    int HealthyAnimals,
    int HealthEvents,
    int UpcomingActivities,
    int Devices,
    decimal Income,
    decimal Expenses,
    decimal Balance);
