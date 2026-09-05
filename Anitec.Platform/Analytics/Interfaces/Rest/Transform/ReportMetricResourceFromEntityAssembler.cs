using Anitec.Platform.Analytics.Domain.Model.Entities;
using Anitec.Platform.Analytics.Interfaces.Rest.Resources;

namespace Anitec.Platform.Analytics.Interfaces.Rest.Transform;

public static class ReportMetricResourceFromEntityAssembler
{
    public static ReportMetricResource ToResourceFromEntity(ReportMetric entity)
    {
        return new ReportMetricResource(entity.Id, entity.Label, entity.Value, entity.Trend);
    }
}
