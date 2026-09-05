namespace Anitec.Platform.Financial.Interfaces.Rest.Resources;

public record FinancialRecordResource(int Id, int OwnerId, string Type, string Category, decimal Amount, DateOnly Date, string Description);
