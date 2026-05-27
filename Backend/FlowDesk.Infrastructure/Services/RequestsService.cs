using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Interfaces;
using FlowDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Infrastructure.Services
{
    public class RequestsService(AppDbContext _context) : IRequestsService
    {
        public async Task<List<MasterRemarks>> GetRequestRemarksAsync(FilterRequestRemarksQueryDto dto)
        {
            return await _context.MasterRemarks.Where(m => (int)m.ActionType == dto.ActionType).ToListAsync();
        }

        public async Task<RequestRespectiveIdResponseDto?> GetRespectiveIds(int requestId)
        {
            return await _context.Requests
                .Where(r => r.RequestId == requestId)
                .Select(r => new RequestRespectiveIdResponseDto
                {
                    RequestId  = r.RequestId,
                    EmployeeId = r.EmployeeId,
                    ManagerId  = r.Employee != null ? r.Employee.ManagerId : null,
                    AssignedToId = r.AssignedToId
                })
                .FirstOrDefaultAsync();
        }
    }
}
