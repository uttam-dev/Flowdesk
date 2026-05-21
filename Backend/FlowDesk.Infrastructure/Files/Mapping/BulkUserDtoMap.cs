using CsvHelper.Configuration;
using FlowDesk.Application.Features.Users.DTOs;

public class BulkUserDtoMap : ClassMap<BulkUserDto>
{
    public BulkUserDtoMap()
    {
        Map(m => m.FullName).Name("FullName");
        Map(m => m.Email).Name("Email");
        Map(m => m.RoleId).Name("RoleId");
        Map(m => m.ManagerId).Name("ManagerId");
        Map(m => m.IsActive).Name("IsActive");

        Map(m => m.RowNumber).Ignore();
    }
}