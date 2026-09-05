namespace Anitec.Platform.Metrics.Domain.Model.Commands;

public record CreateDeviceMetricCommand(int DeviceId, string Type, decimal Value, string Unit, DateTime RecordedAt);
