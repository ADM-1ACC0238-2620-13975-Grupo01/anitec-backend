using Anitec.Platform.Devices.Domain.Model.Commands;

namespace Anitec.Platform.Devices.Domain.Model.Entities;

/// <summary>
/// An IoT device on the farm, such as a scale, smart collar, thermal camera, ear tag or weather station.
/// It can be attached to a whole herd or to a single animal.
/// </summary>
public class Device
{
    public Device()
    {
        Name = string.Empty;
        Type = string.Empty;
        SerialNumber = string.Empty;
        Status = string.Empty;
    }

    public Device(CreateDeviceCommand command)
    {
        Name = command.Name;
        Type = command.Type;
        SerialNumber = command.SerialNumber;
        Status = command.Status;
        HerdId = command.HerdId;
        AnimalId = command.AnimalId;

    }

    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    /// <summary>Manufacturer serial number of the device.</summary>
    public string SerialNumber { get; set; }
    public string Status { get; set; }
    /// <summary>Herd (farm) the device is installed in, if any.</summary>
    public int? HerdId { get; set; }
    /// <summary>Animal wearing the device, if any.</summary>
    public int? AnimalId { get; set; }
}
