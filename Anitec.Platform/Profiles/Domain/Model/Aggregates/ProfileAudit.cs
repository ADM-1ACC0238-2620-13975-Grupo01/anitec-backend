using Anitec.Platform.Shared.Domain.Model.Entities;

namespace Anitec.Platform.Profiles.Domain.Model.Aggregates;

public partial class Profile : IAuditableEntity
{
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
