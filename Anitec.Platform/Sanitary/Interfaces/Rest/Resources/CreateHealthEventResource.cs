namespace Anitec.Platform.Sanitary.Interfaces.Rest.Resources;

public record CreateHealthEventResource(int AnimalId, string Type, DateOnly Date, string Description, string Veterinarian, string Diagnosis, string Treatment, string Prescription, string FollowUp, DateOnly? NextDueDate);
