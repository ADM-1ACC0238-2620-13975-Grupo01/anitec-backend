namespace Anitec.Platform.Activities.Interfaces.Rest.Resources;
// Registro (record) inmutable que representa el recurso de transferencia de datos (DTO) 
// para la creación de una nueva actividad agrícola en las interfaces de la API REST.
public record CreateFarmActivityResource(int? OwnerId, int? VeterinarianId, string Title, string Type, DateOnly Date, string Priority, string Status);
