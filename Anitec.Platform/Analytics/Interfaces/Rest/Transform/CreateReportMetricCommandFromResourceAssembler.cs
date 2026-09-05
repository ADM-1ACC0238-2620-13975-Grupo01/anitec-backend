using Anitec.Platform.Analytics.Domain.Model.Commands;
using Anitec.Platform.Analytics.Interfaces.Rest.Resources;

namespace Anitec.Platform.Analytics.Interfaces.Rest.Transform;

public static class CreateReportMetricCommandFromResourceAssembler
{
    public static CreateReportMetricCommand ToCommandFromResource(CreateReportMetricResource resource)
    {
        return new CreateReportMetricCommand(resource.Label, resource.Value, resource.Trend);
    }
}
