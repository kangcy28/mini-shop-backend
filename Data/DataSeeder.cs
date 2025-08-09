using EcommerceAdminAPI.Models;
using BCrypt.Net;

namespace EcommerceAdminAPI.Data
{
    public static class DataSeeder
    {
        public static void SeedData(AppDbContext context)
        {
            if (context.Database.CanConnect())
            {
                SeedUsers(context);
                SeedCategories(context);
                SeedProducts(context);
                SeedOrders(context);
            }
        }

        private static void SeedUsers(AppDbContext context)
        {
            if (!context.Users.Any())
            {
                var users = new[]
                {
                    new User
                    {
                        Username = "admin",
                        Email = "admin@example.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                        Role = "Admin",
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new User
                    {
                        Username = "user",
                        Email = "user@example.com", 
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
                        Role = "User",
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    }
                };

                context.Users.AddRange(users);
                context.SaveChanges();
            }
        }

        private static void SeedCategories(AppDbContext context)
        {
            if (!context.Categories.Any())
            {
                var categories = new[]
                {
                    new Category { Name = "Electronics" },
                    new Category { Name = "Clothing" },
                    new Category { Name = "Books" },
                    new Category { Name = "Home & Garden" },
                    new Category { Name = "Sports" }
                };

                context.Categories.AddRange(categories);
                context.SaveChanges();
            }
        }

        private static void SeedProducts(AppDbContext context)
        {
            if (!context.Products.Any())
            {
                var products = new[]
                {
                    new Product
                    {
                        Name = "Laptop Computer",
                        Price = 999.99m,
                        Description = "High-performance laptop for work and gaming",
                        CategoryId = 1,
                        Stock = 50,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Smartphone",
                        Price = 699.99m,
                        Description = "Latest model smartphone with advanced features",
                        CategoryId = 1,
                        Stock = 100,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "T-Shirt",
                        Price = 19.99m,
                        Description = "Comfortable cotton t-shirt",
                        CategoryId = 2,
                        Stock = 200,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Programming Book",
                        Price = 39.99m,
                        Description = "Learn programming fundamentals",
                        CategoryId = 3,
                        Stock = 75,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Products.AddRange(products);
                context.SaveChanges();
            }
        }

        private static void SeedOrders(AppDbContext context)
        {
            if (!context.Orders.Any())
            {
                var order = new Order
                {
                    OrderNumber = "ORD-2024-001",
                    TotalAmount = 1019.98m,
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow,
                    OrderDetails = new List<OrderDetail>
                    {
                        new OrderDetail
                        {
                            ProductId = 1,
                            Quantity = 1,
                            UnitPrice = 999.99m
                        },
                        new OrderDetail
                        {
                            ProductId = 3,
                            Quantity = 1,
                            UnitPrice = 19.99m
                        }
                    }
                };

                context.Orders.Add(order);
                context.SaveChanges();
            }
        }
    }
}