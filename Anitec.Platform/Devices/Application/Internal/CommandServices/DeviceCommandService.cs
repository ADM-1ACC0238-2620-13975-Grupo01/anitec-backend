using Anitec.Platform.Devices.Application.CommandServices;
using Anitec.Platform.Devices.Domain.Model.Commands;
using Anitec.Platform.Devices.Domain.Model;
using Anitec.Platform.Devices.Domain.Model.Entities;
using Anitec.Platform.Devices.Domain.Repositories;
using Anitec.Platform.Shared.Application.Model;
using Anitec.Platform.Shared.Domain.Repositories;

namespace Anitec.Platform.Devices.Application.Internal.CommandServices;

/// <summary>
/// Registers, edits and removes IoT devices assigned to a herd or an animal.
/// </summary>
public class DeviceCommandService(IDeviceRepository repository, IUnitOfWork unitOfWork)
    : IDeviceCommandService
{
    /// <summary>Registers a new device from the command.</summary>
    public async Task<Result<Device>> Handle(CreateDeviceCommand command, CancellationToken cancellationToken)
    {
        var entity = new Device(command);
        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<Device>.Success(entity);
    }

    /// <summary>Updates name, type, serial number, status and assignment of an existing device.</summary>
    public async Task<Result<Device>> Handle(UpdateDeviceCommand command, CancellationToken cancellationToken)
    {
        var entity = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (entity is null) return Result<Device>.Failure(DevicesError.DeviceNotFound, "Device not found.");
        entity.Name = command.Name;
        entity.Type = command.Type;
        entity.SerialNumber = command.SerialNumber;
        entity.Status = command.Status;
        entity.HerdId = command.HerdId;
        entity.AnimalId = command.AnimalId;

        repository.Update(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<Device>.Success(entity);
    }

    /// <summary>Deletes a device; fails when the id does not exist.</summary>
    public async Task<Result> Handle(DeleteDeviceCommand command, CancellationToken cancellationToken)
    {
        var entity = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (entity is null) return Result.Failure(DevicesError.DeviceNotFound, "Device not found.");
        repository.Remove(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result.Success();
    }
}
