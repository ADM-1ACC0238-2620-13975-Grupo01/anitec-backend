namespace Anitec.Platform.Financial.Domain.Model.Commands;


public record UpdateFinancialRecordCommand(int Id, int OwnerId, string Type, string Category, decimal Amount, DateOnly Date, string Description);
