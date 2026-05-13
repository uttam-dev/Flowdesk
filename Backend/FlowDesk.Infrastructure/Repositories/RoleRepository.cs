using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Interfaces;
using FlowDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Infrastructure.Repositories
{
    public class RoleRepository(AppDbContext _context) : IRoleRepository
    {
        public async Task<IReadOnlyList<Role>> GetAllAsync()
        {
            return await _context.Roles.AsNoTracking().ToListAsync();
        }

        public Task<Role?> GetByIdAsync(int roleId)
        {
            return _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.RoleId == roleId);
        }

        public Task<Role?> GetByName(string roleName)
        {
            return _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.RoleName == roleName);
        }
    }
}
