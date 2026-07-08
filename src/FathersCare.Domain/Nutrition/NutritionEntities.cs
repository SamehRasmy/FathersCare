using FathersCare.Domain.Common;

namespace FathersCare.Domain.Nutrition;

public sealed class Meal : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string MealType { get; set; } = string.Empty;
}

public sealed class DietPlan : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public sealed class ResidentDietPlan : TenantEntity
{
    public Guid ResidentId { get; set; }
    public Guid DietPlanId { get; set; }
    public DietPlan? DietPlan { get; set; }
    public DateOnly StartsOn { get; set; }
    public DateOnly? EndsOn { get; set; }
}

public sealed class MealDistribution : TenantEntity
{
    public Guid ResidentId { get; set; }
    public Guid MealId { get; set; }
    public Meal? Meal { get; set; }
    public DateOnly DistributionDate { get; set; }
    public bool Delivered { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
}

public sealed class KitchenInventoryItem : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal QuantityOnHand { get; set; }
    public decimal ReorderThreshold { get; set; }
}
