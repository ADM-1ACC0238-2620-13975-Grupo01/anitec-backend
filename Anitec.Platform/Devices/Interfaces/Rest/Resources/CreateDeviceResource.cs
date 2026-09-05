namespace Anitec.Platform.Devices.Interfaces.Rest.Resources;

public record CreateDeviceResource(string Name, string Type, string SerialNumber, string Status, int? HerdId, int? AnimalId);
