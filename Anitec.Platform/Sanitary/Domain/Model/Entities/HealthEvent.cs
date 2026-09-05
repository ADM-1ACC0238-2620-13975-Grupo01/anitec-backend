using Anitec.Platform.Sanitary.Domain.Model.Commands;

namespace Anitec.Platform.Sanitary.Domain.Model.Entities;

public class HealthEvent
{
    public HealthEvent()
    {
        Type = string.Empty;
        Description = string.Empty;
        Veterinarian = string.Empty;
        Diagnosis = string.Empty;
        Treatment = string.Empty;
        Prescription = string.Empty;
        FollowUp = string.Empty;
    }

    public HealthEvent(CreateHealthEventCommand command)
    {
        AnimalId = command.AnimalId;
        Type = command.Type;
        Date = command.Date;
        Description = command.Description;
        Veterinarian = command.Veterinarian;
        Diagnosis = command.Diagnosis;
        Treatment = command.Treatment;
        Prescription = command.Prescription;
        FollowUp = command.FollowUp;
        NextDueDate = command.NextDueDate;

    }

    public int Id { get; set; }
    public int AnimalId { get; set; }
    public string Type { get; set; }
    public DateOnly Date { get; set; }
    public string Description { get; set; }
    public string Veterinarian { get; set; }
    public string Diagnosis { get; set; }
    public string Treatment { get; set; }
    public string Prescription { get; set; }
    public string FollowUp { get; set; }
    public DateOnly? NextDueDate { get; set; }
}
