using Anitec.Platform.Metrics.Domain.Model.Commands;

namespace Anitec.Platform.Metrics.Domain.Model.Entities;

public class DeviceMetric
{
    public DeviceMetric()
    {
        Type = string.Empty;
        Unit = string.Empty;
    }

    public DeviceMetric(CreateDeviceMetricCommand command)
    {
        DeviceId = command.DeviceId;
        Type = command.Type;
        Value = command.Value;
        Unit = command.Unit;
        RecordedAt = command.RecordedAt;

    }

    public int Id { get; set; }
    public int DeviceId { get; set; }
    public string Type { get; set; }
    public decimal Value { get; set; }
    public string Unit { get; set; }
    public DateTime RecordedAt { get; set; }
}
