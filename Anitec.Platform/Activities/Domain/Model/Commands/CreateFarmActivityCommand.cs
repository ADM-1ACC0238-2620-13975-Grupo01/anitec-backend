namespace Anitec.Platform.Activities.Domain.Model.Commands;
// Registro (record) inmutable que encapsula los datos necesarios para crear una nueva actividad agrícola,
// incluyendo identificadores opcionales, título, tipo, fecha, prioridad y estado inicial.
public record CreateFarmActivityCommand(int? OwnerId, int? VeterinarianId, string Title, string Type, DateOnly Date, string Priority, string Status);
