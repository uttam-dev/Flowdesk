using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.Infrastructure.Data.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (!context.Roles.Any())
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                await context.Database.OpenConnectionAsync();
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Roles ON");

                int employeeRoleId = (int)RoleEnum.Employee;
                int managerRoleId = (int)RoleEnum.Manager;
                int adminRoleId = (int)RoleEnum.Admin;
                int supportRoleId = (int)RoleEnum.Support;

                context.Roles.AddRange(
                    new Role { RoleId = employeeRoleId, RoleName = RoleEnum.Employee.ToString() },
                    new Role { RoleId = managerRoleId, RoleName = RoleEnum.Manager.ToString() },
                    new Role { RoleId = adminRoleId, RoleName = RoleEnum.Admin.ToString() },
                    new Role { RoleId = supportRoleId, RoleName = RoleEnum.Support.ToString() }
                );

                await context.SaveChangesAsync();
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Roles OFF");
                await transaction.CommitAsync();
            }

            if (!context.Users.Any())
            {
                context.Users.Add(new User
                {
                    FullName = "Admin User",
                    Email = "admin@test.com",
                    PasswordHash = "hashed",
                    RoleId = 1
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
