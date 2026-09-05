namespace Anitec.Platform.Metrics.Interfaces.Rest.Resources;

public record DeviceMetricResource(int Id, int DeviceId, string Type, decimal Value, string Unit, DateTime RecordedAt);
