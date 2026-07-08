using FathersCare.Domain.Common;

namespace FathersCare.Domain.Finance;

public sealed class Revenue : TenantEntity
{
    public DateOnly Date { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}

public sealed class Expense : TenantEntity
{
    public DateOnly Date { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}

public sealed class OperatingSummary : TenantEntity
{
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpense { get; set; }
}
