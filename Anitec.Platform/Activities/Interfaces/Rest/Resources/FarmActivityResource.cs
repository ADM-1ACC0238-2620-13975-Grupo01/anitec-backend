namespace Anitec.Platform.Activities.Interfaces.Rest.Resources;
// Registro (record) inmutable que representa el recurso de transferencia de datos (DTO) 
// de salida para una actividad agrícola, exponiendo sus propiedades completas hacia las respuestas de la API REST.
public record FarmActivityResource(int Id, int? OwnerId, int? VeterinarianId, string Title, string Type, DateOnly Date, string Priority, string Status);
