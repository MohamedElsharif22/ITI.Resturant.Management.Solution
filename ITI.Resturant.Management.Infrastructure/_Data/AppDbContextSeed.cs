using ITI.Resturant.Management.Domain.Identity;
using ITI.Resturant.Management.Domain.Entities.Menu;
using ITI.Resturant.Management.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.Infrastructure._Data
{
    public class AppDbContextSeed
    {
        public static async Task SeedIdentityDataAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            if (!userManager.Users.Any())
            {
                if (!roleManager.Roles.Any())
                {
                    var adminRole = new IdentityRole("Admin");
                    await roleManager.CreateAsync(adminRole);

                    var userRole = new IdentityRole("User");
                    await roleManager.CreateAsync(userRole);
                }
                else
                {
                    Console.WriteLine($"\n{string.Join(", ", roleManager.Roles.Select(r => r.Name))}\n");
                }

                var user = new ApplicationUser()
                {
                    FirstName = "Muhammad",
                    LastName = "Kamal",
                    UserName = "admin_Muhammad",
                    Email = "admin@byway.com",


                };

                await userManager.CreateAsync(user, "Admin@123");
                await userManager.AddToRoleAsync(user, "Admin");
            }

        }

        // Seed application data for testing (categories, menu items, sample orders)
        public static async Task SeedInitialDataAsync(AppDbContext context)
        {
            if (context == null) return;

            // Seed categories and menu items only when empty
            if (!context.Categories.Any())
            {
                var categories = new List<MenuCategory>
                {
                    new MenuCategory { Name = "Pizzas", Description = "Delicious pizzas", IconClass = "fa-pizza-slice" },
                    new MenuCategory { Name = "Burgers", Description = "Juicy burgers", IconClass = "fa-hamburger" },
                    new MenuCategory { Name = "Drinks", Description = "Refreshing beverages", IconClass = "fa-coffee" }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();

                var pizzasCategory = categories.First(c => c.Name == "Pizzas");
                var burgersCategory = categories.First(c => c.Name == "Burgers");
                var drinksCategory = categories.First(c => c.Name == "Drinks");

                var menuItems = new List<MenuItem>
                {
                    new MenuItem
                    {
                        Name = "Margherita",
                        Description = "Classic cheese and tomato pizza",
                        Price = 9.99m,
                        ImageUrl = "https://example.com/images/margherita.jpg",
                        IsAvailable = true,
                        DailyOrderCount = 0,
                        Category = pizzasCategory
                    },
                    new MenuItem
                    {
                        Name = "Pepperoni",
                        Description = "Spicy pepperoni pizza",
                        Price = 11.99m,
                        ImageUrl = "https://example.com/images/pepperoni.jpg",
                        IsAvailable = true,
                        DailyOrderCount = 0,
                        Category = pizzasCategory
                    },
                    new MenuItem
                    {
                        Name = "Classic Burger",
                        Description = "Beef burger with lettuce and tomato",
                        Price = 8.49m,
                        ImageUrl = "https://example.com/images/classic_burger.jpg",
                        IsAvailable = true,
                        DailyOrderCount = 0,
                        Category = burgersCategory
                    },
                    new MenuItem
                    {
                        Name = "Coke",
                        Description = "Chilled Coca-Cola",
                        Price = 1.99m,
                        ImageUrl = "https://example.com/images/coke.jpg",
                        IsAvailable = true,
                        DailyOrderCount = 0,
                        Category = drinksCategory
                    }
                };

                await context.MenuItems.AddRangeAsync(menuItems);
                await context.SaveChangesAsync();
            }

            // You can add more seed data for Orders or other entities if needed
        }
    }
}
