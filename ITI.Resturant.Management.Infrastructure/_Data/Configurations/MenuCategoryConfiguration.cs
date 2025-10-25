using ITI.Resturant.Management.Domain.Entities.Menu;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITI.Resturant.Management.Infrastructure._Data.Configurations
{
    public class MenuCategoryConfiguration : IEntityTypeConfiguration<MenuCategory>
    {
        public void Configure(EntityTypeBuilder<MenuCategory> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
            builder.Property(c => c.Description).HasMaxLength(500);
            builder.Property(c => c.IconClass).HasMaxLength(100);
            
            builder.HasQueryFilter(c => !c.IsDeleted);

            // Configure relationship with MenuItems
            builder.HasMany(c => c.MenuItems)
                .WithOne(mi => mi.Category)
                .HasForeignKey("CategoryId")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}