using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Interfaces;
using FlowDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FlowDesk.Infrastructure.Repositories
{
    public class UnitOfWork(AppDbContext _context) : IUnitOfWork
    {
        public async Task<IReadOnlyList<RoleResponseDto>> GetAllRoles()
        {
            var roles =  await _context.Roles.Select(r => new RoleResponseDto
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName
            }).ToListAsync();
            return roles;
        }
    }
}
