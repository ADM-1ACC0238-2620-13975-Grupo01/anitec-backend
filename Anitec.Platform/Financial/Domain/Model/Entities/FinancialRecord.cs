using Anitec.Platform.Financial.Domain.Model.Commands;

namespace Anitec.Platform.Financial.Domain.Model.Entities;

/// <summary>
/// An income or expense entry recorded by a rancher.
/// </summary>
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
    /// <summary>Id of the rancher the record belongs to.</summary>
    public int OwnerId { get; set; }
    /// <summary>Whether the record is an income or an expense.</summary>
    public string Type { get; set; }
    /// <summary>Grouping used for reports, such as feed, sales or veterinary care.</summary>
    public string Category { get; set; }
    /// <summary>Amount of money of the record.</summary>
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public string Description { get; set; }
}
