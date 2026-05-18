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
    }
}
