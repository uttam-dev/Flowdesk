using FlowDesk.Application.Services;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Utils;
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
                    new Role { RoleId = employeeRoleId, RoleName = RoleName.Employee },
                    new Role { RoleId = managerRoleId, RoleName = RoleName.Manager },
                    new Role { RoleId = adminRoleId, RoleName = RoleName.Admin },
                    new Role { RoleId = supportRoleId, RoleName = RoleName.Support }
                );

                await context.SaveChangesAsync();
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Roles OFF");
                await transaction.CommitAsync();
            }

            if (!context.Users.Any())
            {
                var passHashed = PasswordService.HashPassword("12345");

                context.Users.Add(new User
                {
                    FullName = "Admin User",
                    Email = "admin@test.com",
                    PasswordHash = passHashed,
                    RoleId = 3
                });

                var dummyUsers = new List<User>
                {
                    new User { FullName = "David Brown",    Email = "david@test.com",  PasswordHash = passHashed, RoleId = 1 },
                    new User { FullName = "Eva Martinez",   Email = "eva@test.com",    PasswordHash = passHashed, RoleId = 1 },
                    new User { FullName = "Frank Wilson",   Email = "frank@test.com",  PasswordHash = passHashed, RoleId = 1 },
                    new User { FullName = "Grace Lee",      Email = "grace@test.com",  PasswordHash = passHashed, RoleId = 1 },
                    new User { FullName = "Alice Johnson",  Email = "alice@test.com",  PasswordHash = passHashed, RoleId = 2 },
                    new User { FullName = "Bob Smith",      Email = "bob@test.com",    PasswordHash = passHashed, RoleId = 2 },
                    new User { FullName = "Carol White",    Email = "carol@test.com",  PasswordHash = passHashed, RoleId = 2 },
                    new User { FullName = "Henry Taylor",   Email = "henry@test.com",  PasswordHash = passHashed, RoleId = 4 },
                    new User { FullName = "Isla Anderson",  Email = "isla@test.com",   PasswordHash = passHashed, RoleId = 4 },
                    new User { FullName = "Jack Thomas",    Email = "jack@test.com",   PasswordHash = passHashed, RoleId = 4 },
                };

                context.Users.AddRange(dummyUsers);
            }

            await context.SaveChangesAsync();
        }
    }
}
