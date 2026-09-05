using Anitec.Platform.Analytics.Domain.Model.Commands;
using Anitec.Platform.Analytics.Interfaces.Rest.Resources;

namespace Anitec.Platform.Analytics.Interfaces.Rest.Transform;

public static class UpdateReportMetricCommandFromResourceAssembler
{
    public static UpdateReportMetricCommand ToCommandFromResource(int id, CreateReportMetricResource resource)
    {
        return new UpdateReportMetricCommand(id, resource.Label, resource.Value, resource.Trend);
    }
}
