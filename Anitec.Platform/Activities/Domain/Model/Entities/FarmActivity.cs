using Anitec.Platform.Activities.Domain.Model.Commands;

namespace Anitec.Platform.Activities.Domain.Model.Entities;

/// <summary>
/// A task scheduled on a farm, such as a vaccination round or a feeding, owned by a rancher
/// and optionally assigned to a veterinarian.
/// </summary>
public class FarmActivity
{
    public FarmActivity()
    {
        Title = string.Empty;
        Type = string.Empty;
        Priority = string.Empty;
        Status = string.Empty;
    }

    /// <summary>Creates a farm activity from the values of the create command.</summary>
    public FarmActivity(CreateFarmActivityCommand command)
    {
        OwnerId = command.OwnerId;
        VeterinarianId = command.VeterinarianId;
        Title = command.Title;
        Type = command.Type;
        Date = command.Date;
        Priority = command.Priority;
        Status = command.Status;

    }

    public int Id { get; set; }
    /// <summary>Id of the rancher who owns the activity.</summary>
    public int? OwnerId { get; set; }
    /// <summary>Id of the veterinarian assigned to the activity, if any.</summary>
    public int? VeterinarianId { get; set; }
    public string Title { get; set; }
    public string Type { get; set; }
    /// <summary>Day the activity is scheduled for.</summary>
    public DateOnly Date { get; set; }
    public string Priority { get; set; }
    public string Status { get; set; }
}
