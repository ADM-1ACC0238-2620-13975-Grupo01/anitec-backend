namespace Anitec.Platform.Devices.Interfaces.Rest.Resources;

public record DeviceResource(int Id, string Name, string Type, string SerialNumber, string Status, int? HerdId, int? AnimalId);
