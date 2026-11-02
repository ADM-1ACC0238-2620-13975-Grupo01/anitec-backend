using Anitec.Platform.Sanitary.Domain.Model.Commands;

namespace Anitec.Platform.Sanitary.Domain.Model.Entities;

/// <summary>
/// An entry in an animal's medical history, such as a vaccination, a check-up or a treatment.
/// </summary>
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
    /// <summary>Animal the event belongs to.</summary>
    public int AnimalId { get; set; }
    public string Type { get; set; }
    public DateOnly Date { get; set; }
    public string Description { get; set; }
    /// <summary>Name of the veterinarian who attended the animal.</summary>
    public string Veterinarian { get; set; }
    public string Diagnosis { get; set; }
    public string Treatment { get; set; }
    public string Prescription { get; set; }
    /// <summary>Follow-up instructions after the event.</summary>
    public string FollowUp { get; set; }
    /// <summary>Date of the next dose or check-up, if one is needed.</summary>
    public DateOnly? NextDueDate { get; set; }
}
