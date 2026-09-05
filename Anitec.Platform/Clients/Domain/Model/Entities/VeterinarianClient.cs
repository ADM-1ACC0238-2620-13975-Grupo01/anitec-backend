using Anitec.Platform.Clients.Domain.Model.Commands;

namespace Anitec.Platform.Clients.Domain.Model.Entities;


public class VeterinarianClient
{
    public VeterinarianClient()
    {
        Status = "Accepted";
        RequestedAt = DateTime.UtcNow;
    }

    public VeterinarianClient(CreateVeterinarianClientCommand command)
    {
        VeterinarianId = command.VeterinarianId;
        RancherId = command.RancherId;
        Status = string.IsNullOrWhiteSpace(command.Status) ? "Accepted" : command.Status;
        RequestedAt = DateTime.UtcNow;
        AcceptedAt = Status.Equals("Accepted", StringComparison.OrdinalIgnoreCase) ? DateTime.UtcNow : null;
    }

    public int Id { get; set; }
    public int VeterinarianId { get; set; }
    public int RancherId { get; set; }
    public string Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
}
