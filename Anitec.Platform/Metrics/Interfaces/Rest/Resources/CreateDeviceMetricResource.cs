namespace Anitec.Platform.Metrics.Interfaces.Rest.Resources;

public record CreateDeviceMetricResource(int DeviceId, string Type, decimal Value, string Unit, DateTime RecordedAt);
