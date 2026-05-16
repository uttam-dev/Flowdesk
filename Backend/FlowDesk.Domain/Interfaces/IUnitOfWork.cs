using FlowDesk.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        Task<IReadOnlyList<RoleResponseDto>> GetAllRoles();
    }
}
