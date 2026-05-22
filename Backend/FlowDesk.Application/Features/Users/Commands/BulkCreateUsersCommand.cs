using FlowDesk.Application.Features.Users.DTOs;
using FlowDesk.Application.Services;
using FlowDesk.Domain.Entities;
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

            var validUsers = new List<User>();

            // Duplicate in file
            var duplicateEmails = request.Users
                .Where(x => !string.IsNullOrWhiteSpace(x.Email))
                .GroupBy(x => x.Email.Trim().ToLower())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToHashSet();

            // DB duplicates (ONE QUERY)
            var emails = request.Users
                .Where(x => !string.IsNullOrWhiteSpace(x.Email))
                .Select(x => x.Email.Trim().ToLower())
                .Distinct()
                .ToList();

            var existingEmails = await userRepository.GetExistingEmailsAsync(emails);
            var existingSet = existingEmails.ToHashSet();
            var passwordHash = PasswordService.HashPassword("12345678");

            foreach (var item in request.Users)
            {
                var email = item.Email?.Trim().ToLower();

                // Validation
                if (string.IsNullOrWhiteSpace(item.FullName))
                {
                    AddError(response, item, "FullName is required");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    AddError(response, item, "Email is required");
                    continue;
                }

                if (!email.Contains("@"))
                {
                    AddError(response, item, "Invalid email format");
                    continue;
                }

                if (duplicateEmails.Contains(email))
                {
                    AddError(response, item, "Duplicate email in file");
                    continue;
                }

                if (existingSet.Contains(email))
                {
                    AddError(response, item, "Email already exists");
                    continue;
                }

                if (item.RoleId <= 0)
                {
                    AddError(response, item, "Invalid RoleId");
                    continue;
                }

                validUsers.Add(new User
                {
                    FullName = item.FullName.Trim(),
                    Email = email,
                    RoleId = item.RoleId,
                    ManagerId = item.ManagerId,
                    IsActive = item.IsActive ?? true,
                    PasswordHash = passwordHash,
                    CreatedOn = DateTime.UtcNow
                });
            }

            // Bulk Insert
            if (validUsers.Any())
            {
                await userRepository.BulkInsertAsync(validUsers);
            }

            response.SuccessCount = validUsers.Count;
            response.FailedCount = response.Total - response.SuccessCount;

            logger.LogInformation("Bulk users insert done. Success: {Success}, Failed: {Failed}",
                response.SuccessCount, response.FailedCount);

            return response;
        }

        private void AddError(BulkCreateUsersResponse response, BulkUserDto item, string error)
        {
            response.Errors.Add(new BulkUserError
            {
                RowNumber = item.RowNumber,
                Email = item.Email ?? "",
                Error = error
            });
        }
    }
}
