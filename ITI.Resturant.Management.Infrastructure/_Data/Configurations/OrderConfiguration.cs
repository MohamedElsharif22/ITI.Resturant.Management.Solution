using ITI.Resturant.Management.Domain.Entities.Order_;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITI.Resturant.Management.Infrastructure._Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.CustomerName).IsRequired().HasMaxLength(100);
            builder.Property(o => o.CustomerPhone).IsRequired().HasMaxLength(20);
            builder.Property(o => o.CustomerEmail).IsRequired().HasMaxLength(100);
            builder.Property(o => o.DeliveryAddress).HasMaxLength(500);
            builder.Property(o => o.SpecialInstructions).HasMaxLength(500);
            
            builder.Property(o => o.Subtotal).HasPrecision(18, 2);
            builder.Property(o => o.Tax).HasPrecision(18, 2);
            builder.Property(o => o.Discount).HasPrecision(18, 2);
            builder.Property(o => o.Total).HasPrecision(18, 2);

            builder.Property(o => o.OrderDate).IsRequired();
            builder.Property(o => o.Status).IsRequired();
            builder.Property(o => o.OrderType).IsRequired();

            builder.HasQueryFilter(o => !o.IsDeleted);
        }
    }
}