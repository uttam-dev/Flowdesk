using FlowDesk.Application.Services;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Utils;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

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

            if (!context.MasterRemarks.Any())
            {
                context.MasterRemarks.AddRange(

                        // APPROVE (1)
                        new MasterRemarks { RemarksText = "Valid request", ActionType = RemarksActionTypeEnum.Approve },
                        new MasterRemarks { RemarksText = "Approved as per policy", ActionType = RemarksActionTypeEnum.Approve },
                        new MasterRemarks { RemarksText = "Budget available", ActionType = RemarksActionTypeEnum.Approve },
                        new MasterRemarks { RemarksText = "Meets criteria", ActionType = RemarksActionTypeEnum.Approve },
                        new MasterRemarks { RemarksText = "Manager approved", ActionType = RemarksActionTypeEnum.Approve },

                        // REJECT (2)
                        new MasterRemarks { RemarksText = "Invalid request", ActionType = RemarksActionTypeEnum.Reject },
                        new MasterRemarks { RemarksText = "Insufficient details", ActionType = RemarksActionTypeEnum.Reject },
                        new MasterRemarks { RemarksText = "Duplicate request", ActionType = RemarksActionTypeEnum.Reject },
                        new MasterRemarks { RemarksText = "Policy violation", ActionType = RemarksActionTypeEnum.Reject },
                        new MasterRemarks { RemarksText = "Budget not approved", ActionType = RemarksActionTypeEnum.Reject },
                        new MasterRemarks { RemarksText = "Request not required", ActionType = RemarksActionTypeEnum.Reject },

                        // ASSIGN (3)
                        new MasterRemarks { RemarksText = "Assigned to support", ActionType = RemarksActionTypeEnum.Assign },
                        new MasterRemarks { RemarksText = "Assigned based on workload", ActionType = RemarksActionTypeEnum.Assign },
                        new MasterRemarks { RemarksText = "Assigned to appropriate team", ActionType = RemarksActionTypeEnum.Assign },
                        new MasterRemarks { RemarksText = "Urgent assignment", ActionType = RemarksActionTypeEnum.Assign },

                        // STATUS CHANGE (4) - InProgress
                        new MasterRemarks { RemarksText = "Work started", ActionType = RemarksActionTypeEnum.RequestChanges },
                        new MasterRemarks { RemarksText = "Investigation in progress", ActionType = RemarksActionTypeEnum.RequestChanges },
                        new MasterRemarks { RemarksText = "Task picked up", ActionType = RemarksActionTypeEnum.RequestChanges },
                        new MasterRemarks { RemarksText = "Processing request", ActionType = RemarksActionTypeEnum.RequestChanges },

                        // STATUS CHANGE (4) - Resolved
                        new MasterRemarks { RemarksText = "Issue resolved", ActionType = RemarksActionTypeEnum.RequestChanges },
                        new MasterRemarks { RemarksText = "Request completed", ActionType = RemarksActionTypeEnum.RequestChanges },
                        new MasterRemarks { RemarksText = "Service delivered", ActionType = RemarksActionTypeEnum.RequestChanges },
                        new MasterRemarks { RemarksText = "Fixed successfully", ActionType = RemarksActionTypeEnum.RequestChanges }
                    );
            }
            await context.SaveChangesAsync();
        }
    }
}
