using FathersCare.Domain.Nutrition;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FathersCare.Infrastructure.Persistence.Configurations;

public sealed class MealConfiguration : IEntityTypeConfiguration<Meal>
{
    public void Configure(EntityTypeBuilder<Meal> builder)
    {
        builder.Property(meal => meal.Name).HasMaxLength(160).IsRequired();
        builder.Property(meal => meal.MealType).HasMaxLength(80).IsRequired();
    }
}
