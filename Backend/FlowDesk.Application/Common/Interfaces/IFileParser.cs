using FlowDesk.Application.Features.Users.DTOs;

namespace FlowDesk.Application.Common.Interfaces
{
    public interface IFileParser
    {
        Task<List<BulkUserDto>> ParseAsync(Stream stream, string fileName);
    }
}
