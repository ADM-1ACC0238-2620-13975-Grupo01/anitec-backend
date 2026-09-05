namespace Anitec.Platform.Sanitary.Domain.Model.Commands;

public record UpdateHealthEventCommand(int Id, int AnimalId, string Type, DateOnly Date, string Description, string Veterinarian, string Diagnosis, string Treatment, string Prescription, string FollowUp, DateOnly? NextDueDate);
