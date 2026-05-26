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
                    new Role { RoleId = employeeRoleId, RoleName = RoleEnum.Employee.ToString() },
                    new Role { RoleId = managerRoleId, RoleName = RoleEnum.Manager.ToString() },
                    new Role { RoleId = adminRoleId, RoleName = RoleEnum.Admin.ToString() },
                    new Role { RoleId = supportRoleId, RoleName = RoleEnum.Support.ToString() }
                );

                await context.SaveChangesAsync();
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Roles OFF");
                await transaction.CommitAsync();
            }

            // Only on development environment, remove in production
            if (!context.Users.Any())
            {
                var passHashed = PasswordService.HashPassword("12345678");

                context.Users.Add(new User
                {
                    FullName = "Admin User",
                    Email = "admin@flowdesk.in",
                    PasswordHash = passHashed,
                    RoleId = 3
                });

                var dummyUsers = new List<User>
                {
                    new User { FullName = "Priya Desai",        Email = "priya.desai@flowdesk.in",  PasswordHash = passHashed, RoleId = 2 },
                    new User { FullName = "Neha Joshi",         Email = "neha.joshi@flowdesk.in",    PasswordHash = passHashed, RoleId = 2 },
                    new User { FullName = "Vikas Yadav",        Email = "vikas.yadav@flowdesk.in",  PasswordHash = passHashed, RoleId = 2 },
                    new User { FullName = "Amit Sharma",        Email = "amit.sharma@flowdesk.in",  PasswordHash = passHashed, RoleId = 2 },
                    new User { FullName = "Nayan Patel",        Email = "nayan.patel@flowdesk.in",    PasswordHash = passHashed, RoleId = 2 },
                    new User { FullName = "Rohit Mehta",        Email = "rohit.mehta@flowdesk.in",  PasswordHash = passHashed, RoleId = 2 },
                    new User { FullName = "Pooja Verma",        Email = "pooja.verma@flowdesk.in",  PasswordHash = passHashed, RoleId = 4 },
                    new User { FullName = "Ankit Gupta",        Email = "ankit.gupta@flowdesk.in",   PasswordHash = passHashed, RoleId = 4 },
                    new User { FullName = "Rahul Nair",         Email = "rahul.nair@flowdesk.in",   PasswordHash = passHashed, RoleId = 4 },
                    new User { FullName = "Uttam Prajapati",    Email = "uttam.prajapati@flowdesk.in",   PasswordHash = passHashed, RoleId = 3 },
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

            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                        new Category { CategoryName = "IT Support", IsApprovalRequired = false, SLAHours = 24 },
                        new Category { CategoryName = "Hardware Issue", IsApprovalRequired = false, SLAHours = 12 },
                        new Category { CategoryName = "Software Installation", IsApprovalRequired = true, SLAHours = 48 },
                        new Category { CategoryName = "Network Issue", IsApprovalRequired = false, SLAHours = 8 },
                        new Category { CategoryName = "Access Request", IsApprovalRequired = true, SLAHours = 24 },
                        new Category { CategoryName = "Email Issue", IsApprovalRequired = false, SLAHours = 6 },
                        new Category { CategoryName = "VPN Issue", IsApprovalRequired = false, SLAHours = 8 },
                        new Category { CategoryName = "System Crash", IsApprovalRequired = false, SLAHours = 4 },
                        new Category { CategoryName = "Security Issue", IsApprovalRequired = true, SLAHours = 6 },
                        new Category { CategoryName = "Printer Issue", IsApprovalRequired = false, SLAHours = 12 }
                    );
            }
            await context.SaveChangesAsync();
        }
    }
}
