namespace Anitec.Platform.Metrics.Domain.Model.Commands;

public record UpdateDeviceMetricCommand(int Id, int DeviceId, string Type, decimal Value, string Unit, DateTime RecordedAt);
