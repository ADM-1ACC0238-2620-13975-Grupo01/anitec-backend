namespace Anitec.Platform.Financial.Interfaces.Rest.Resources;

public record CreateFinancialRecordResource(int OwnerId, string Type, string Category, decimal Amount, DateOnly Date, string Description);
