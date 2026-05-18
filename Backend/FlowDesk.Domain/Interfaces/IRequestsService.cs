using FlowDesk.Domain.Entities;
using FlowDesk.Domain.DTOs;

namespace FlowDesk.Domain.Interfaces
{
    public interface IRequestsService
    {
        Task<List<MasterRemarks>> GetRequestRemarksAsync(FilterRequestRemarksQueryDto dto);
    }
}
