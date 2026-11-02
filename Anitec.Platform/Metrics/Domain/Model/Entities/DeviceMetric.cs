using Anitec.Platform.Metrics.Domain.Model.Commands;

namespace Anitec.Platform.Metrics.Domain.Model.Entities;

/// <summary>
/// A single reading sent by a device, such as a weight or a body temperature.
/// </summary>
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
    /// <summary>Device that produced the reading.</summary>
    public int DeviceId { get; set; }
    /// <summary>What was measured.</summary>
    public string Type { get; set; }
    public decimal Value { get; set; }
    /// <summary>Unit of <see cref="Value"/>, for example kg or °C.</summary>
    public string Unit { get; set; }
    /// <summary>Moment the reading was taken.</summary>
    public DateTime RecordedAt { get; set; }
}
