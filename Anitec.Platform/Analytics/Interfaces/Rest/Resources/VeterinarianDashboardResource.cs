namespace Anitec.Platform.Analytics.Interfaces.Rest.Resources;

public record VeterinarianDashboardResource(
    int Clients,
    int Herds,
    int Patients,
    int HealthEvents,
    int UpcomingVisits,
    int ActiveTreatments);
