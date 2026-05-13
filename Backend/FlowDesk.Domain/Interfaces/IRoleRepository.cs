using FlowDesk.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Domain.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role?> GetByIdAsync(int roleId);
        Task<Role?> GetByName(string roleName);
        Task<IReadOnlyList<Role>> GetAllAsync();
    }
}
