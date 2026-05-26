using CsvHelper.Configuration;
using FlowDesk.Application.Features.Users.DTOs;

namespace FlowDesk.Infrastructure.Files.Mapping
{
    public class BulkUserDtoMap : ClassMap<BulkUserDto>
    {
        public BulkUserDtoMap()
        {
            Map(m => m.FullName).Name("FullName");
            Map(m => m.Email).Name("Email");

            Map(m => m.RoleName).Name("RoleName");
            Map(m => m.ManagerEmail).Name("ManagerEmail");

            Map(m => m.IsActive).Name("IsActive").Optional();

            Map(m => m.RowNumber).Ignore();
        }
    }
}