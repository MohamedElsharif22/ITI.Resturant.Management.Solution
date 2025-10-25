using ITI.Resturant.Management.Domain.Entities.Menu;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITI.Resturant.Management.Infrastructure._Data.Configurations
{
    public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
    {
        public void Configure(EntityTypeBuilder<MenuItem> builder)
        {
            builder.HasKey(mi => mi.Id);

            builder.Property(mi => mi.Name).IsRequired().HasMaxLength(100);
            builder.Property(mi => mi.Description).HasMaxLength(500);
            builder.Property(mi => mi.Price).HasPrecision(18, 2).IsRequired();
            builder.Property(mi => mi.ImageUrl).HasMaxLength(500);

            builder.HasQueryFilter(mi => !mi.IsDeleted);

            // Configure relationship with MenuCategory
            builder.HasOne(mi => mi.Category)
                .WithMany(c => c.MenuItems)
                .HasForeignKey("CategoryId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}