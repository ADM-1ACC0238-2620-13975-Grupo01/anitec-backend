namespace Anitec.Platform.Activities.Domain.Model.Commands;
// Registro (record) inmutable que encapsula el identificador y los datos actualizados para modificar una actividad agrícola existente.
public record UpdateFarmActivityCommand(int Id, int? OwnerId, int? VeterinarianId, string Title, string Type, DateOnly Date, string Priority, string Status);
