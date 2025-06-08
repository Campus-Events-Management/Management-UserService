using EventManagement.UserService.Models;
using EventManagement.UserService.Services;

namespace EventManagement.UserService.Data
{
    public static class DbInitializer
    {
        public static void Initialize(UserDbContext context, IPasswordHasher passwordHasher)
        {
            context.Database.EnsureCreated();

            // Check if users already exist
            if (context.Users.Any())
            {
                return; // DB has been seeded
            }

            // Create sample users
            var users = new User[]
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin User",
                    Email = "admin@example.com",
                    PasswordHash = passwordHasher.HashPassword("Admin@123"),
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "John Doe",
                    Email = "john@example.com",
                    PasswordHash = passwordHasher.HashPassword("Password@123"),
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Jane Smith",
                    Email = "jane@example.com",
                    PasswordHash = passwordHasher.HashPassword("Password@123"),
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }
    }
} 