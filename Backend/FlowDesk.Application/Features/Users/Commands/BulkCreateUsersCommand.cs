using FlowDesk.Application.Features.Users.DTOs;
using FlowDesk.Application.Services;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;


namespace FlowDesk.Application.Features.Users.Commands
{

    public class BulkCreateUsersCommand : IRequest<BulkCreateUsersResponse>
    {
        public List<BulkUserDto> Users { get; set; } = new();
    }

    public class BulkCreateUsersHandler(
       IUserRepository userRepository,
       ILogger<BulkCreateUsersHandler> logger)
       : IRequestHandler<BulkCreateUsersCommand, BulkCreateUsersResponse>
    {
        public async Task<BulkCreateUsersResponse> Handle(
            BulkCreateUsersCommand request,
            CancellationToken cancellationToken)
        {
            var response = new BulkCreateUsersResponse
            {
                Total = request.Users.Count
            };

            if (!request.Users.Any())
                return response;

            var now = DateTime.UtcNow;
            var validUsers = new List<User>();

            // Track emails already processed in this batch
            var processedEmails = new HashSet<string>();

            // ---------------- NORMALIZE ----------------
            var users = request.Users.Select(x =>
            {
                x.FullName = x.FullName?.Trim();
                x.Email = x.Email?.Trim().ToLower();
                x.ManagerEmail = x.ManagerEmail?.Trim().ToLower();
                x.RoleName = x.RoleName?.Trim();
                return x;
            }).ToList();

            // ---------------- DUPLICATE IN FILE ----------------
            var duplicateEmails = users
                .Where(x => !string.IsNullOrWhiteSpace(x.Email))
                .GroupBy(x => x.Email)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key!)
                .ToHashSet();

            // ---------------- EMAIL LISTS ----------------
            var allEmails = users
                .Where(x => !string.IsNullOrWhiteSpace(x.Email))
                .Select(x => x.Email!)
                .Distinct()
                .ToList();

            var managerEmails = users
                .Where(x => !string.IsNullOrWhiteSpace(x.ManagerEmail))
                .Select(x => x.ManagerEmail!)
                .Distinct()
                .ToList();

            // ---------------- DB CALLS ----------------
            var existingEmails = await userRepository
                .GetExistingEmailsAsync(allEmails);

            var existingEmailSet = existingEmails
                .Select(x => x.ToLower())
                .ToHashSet();

            var managersFromDb = await userRepository
                .GetUsersByEmailsAsync(managerEmails);

            var managerDict = managersFromDb
                .GroupBy(x => x.Email.ToLower())
                .ToDictionary(g => g.Key, g => g.First().UserId);

            // ---------------- PASSWORD ----------------
            var passwordHash = PasswordService.HashPassword("12345678");

            // ---------------- PROCESS ----------------
            foreach (var item in users)
            {
                var email = item.Email;

                void AddError(string error)
                {
                    response.Errors.Add(new BulkUserError
                    {
                        RowNumber = item.RowNumber,
                        Email = item.Email ?? "",
                        Error = error
                    });
                }

                // ---------- BASIC ----------
                if (string.IsNullOrWhiteSpace(item.FullName))
                {
                    AddError("FullName is required");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    AddError("Email is required");
                    continue;
                }

                if (!IsValidEmail(email))
                {
                    AddError("Invalid email format");
                    continue;
                }

                if (duplicateEmails.Contains(email))
                {
                    AddError("Duplicate email in file");
                    continue;
                }

                if (existingEmailSet.Contains(email))
                {
                    AddError("Email already exists in DB");
                    continue;
                }

                if (!processedEmails.Add(email))
                {
                    AddError("Duplicate email in same batch");
                    continue;
                }

                // ---------- ROLE ----------
                if (!Enum.TryParse<RoleEnum>(item.RoleName, true, out var roleEnum))
                {
                    AddError("Invalid role");
                    continue;
                }

                var roleId = (int)roleEnum;

                // ---------- MANAGER ----------
                int? managerId = null;

                if (roleEnum == RoleEnum.Employee)
                {
                    if (string.IsNullOrWhiteSpace(item.ManagerEmail))
                    {
                        AddError("Employee must have manager");
                        continue;
                    }

                    // 1. Check DB
                    if (managerDict.TryGetValue(item.ManagerEmail!, out var dbManagerId))
                    {
                        managerId = dbManagerId;
                    }
                    // 2. Check SAME FILE (already processed)
                    else if (processedEmails.Contains(item.ManagerEmail!))
                    {
                        var manager = validUsers
                            .FirstOrDefault(x => x.Email == item.ManagerEmail);

                        if (manager != null)
                            managerId = manager.UserId; // will be 0 until saved → depends on your repo
                    }
                    else
                    {
                        AddError("Manager not found");
                        continue;
                    }

                    if (managerId == null)
                    {
                        AddError("Manager resolution failed");
                        continue;
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(item.ManagerEmail))
                    {
                        AddError("Only Employee can have manager");
                        continue;
                    }
                }

                // ---------- ADD ----------
                validUsers.Add(new User
                {
                    FullName = item.FullName!,
                    Email = email!,
                    RoleId = roleId,
                    ManagerId = managerId,
                    IsActive = item.IsActive ?? true,
                    PasswordHash = passwordHash,
                    CreatedOn = now
                });
            }

            // ---------------- INSERT ----------------
            if (validUsers.Any())
                await userRepository.BulkInsertAsync(validUsers);

            response.SuccessCount = validUsers.Count;
            response.FailedCount = response.Total - response.SuccessCount;

            logger.LogInformation(
                "Bulk upload done. Total: {Total}, Success: {Success}, Failed: {Failed}",
                response.Total,
                response.SuccessCount,
                response.FailedCount);

            return response;
        }

        // ---------------- EMAIL VALIDATION ----------------
        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
