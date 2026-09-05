using Anitec.Platform.Financial.Domain.Model.Commands;

namespace Anitec.Platform.Financial.Domain.Model.Entities;


public class FinancialRecord
{
    public FinancialRecord()
    {
        Type = string.Empty;
        Category = string.Empty;
        Description = string.Empty;
    }

    public FinancialRecord(CreateFinancialRecordCommand command)
    {
        OwnerId = command.OwnerId;
        Type = command.Type;
        Category = command.Category;
        Amount = command.Amount;
        Date = command.Date;
        Description = command.Description;

    }

    public int Id { get; set; }
    public int OwnerId { get; set; }
    public string Type { get; set; }
    public string Category { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public string Description { get; set; }
}
