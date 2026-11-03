namespace Anitec.Platform.Activities.Domain.Model.Commands;
// Registro (record) inmutable que encapsula el identificador único necesario para eliminar una actividad agrícola existente.
public record DeleteFarmActivityCommand(int Id);
