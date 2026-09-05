namespace Anitec.Platform.Financial.Domain.Model.Commands;
//
public record CreateFinancialRecordCommand(int OwnerId, string Type, string Category, decimal Amount, DateOnly Date, string Description);
