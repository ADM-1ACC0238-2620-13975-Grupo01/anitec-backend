namespace Anitec.Platform.Devices.Domain.Model.Commands;

public record CreateDeviceCommand(string Name, string Type, string SerialNumber, string Status, int? HerdId, int? AnimalId);
