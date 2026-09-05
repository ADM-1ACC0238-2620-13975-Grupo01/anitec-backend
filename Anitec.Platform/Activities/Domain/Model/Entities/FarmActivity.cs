using Anitec.Platform.Activities.Domain.Model.Commands;

namespace Anitec.Platform.Activities.Domain.Model.Entities;

public class FarmActivity
{
    public FarmActivity()
    {
        Title = string.Empty;
        Type = string.Empty;
        Priority = string.Empty;
        Status = string.Empty;
    }

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
    public int? OwnerId { get; set; }
    public int? VeterinarianId { get; set; }
    public string Title { get; set; }
    public string Type { get; set; }
    public DateOnly Date { get; set; }
    public string Priority { get; set; }
    public string Status { get; set; }
}
