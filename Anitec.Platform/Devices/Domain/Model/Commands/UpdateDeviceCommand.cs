namespace Anitec.Platform.Devices.Domain.Model.Commands;

public record UpdateDeviceCommand(int Id, string Name, string Type, string SerialNumber, string Status, int? HerdId, int? AnimalId);
