using CsvHelper;
using CsvHelper.Configuration;
using FlowDesk.Application.Common.Interfaces;
using FlowDesk.Application.Features.Users.DTOs;
using System.Globalization;

namespace FlowDesk.Infrastructure.Services
{
    public class FileParser : IFileParser
    {
        public async Task<List<BulkUserDto>> ParseAsync(Stream stream, string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();

            return ext switch
            {
                ".csv" => ParseCsv(stream),
                ".xlsx" => ParseExcel(stream),
                _ => throw new Exception("Unsupported file format")
            };
        }

        private List<BulkUserDto> ParseCsv(Stream stream)
        {
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            // REGISTER CLASS MAP HERE
            csv.Context.RegisterClassMap<BulkUserDtoMap>();

            var records = csv.GetRecords<BulkUserDto>().ToList();

            // Assign RowNumber manually
            for (int i = 0; i < records.Count; i++)
                records[i].RowNumber = i + 2;

            return records;
        }

        private List<BulkUserDto> ParseExcel(Stream stream)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
            var sheet = workbook.Worksheet(1);

            var rows = sheet.RangeUsed().RowsUsed().Skip(1);

            var list = new List<BulkUserDto>();
            int rowNum = 2;

            foreach (var row in rows)
            {
                list.Add(new BulkUserDto
                {
                    RowNumber = rowNum++,
                    FullName = row.Cell(1).GetString(),
                    Email = row.Cell(2).GetString(),
                    RoleId = row.Cell(3).GetValue<int>(),
                    ManagerId = row.Cell(4).GetValue<int?>(),
                    IsActive = row.Cell(5).GetValue<bool?>()
                });
            }

            return list;
        }
    }

    // KEEP THIS IN SAME FILE OR SEPARATE (Infrastructure Layer)
    public sealed class BulkUserDtoMap : ClassMap<BulkUserDto>
    {
        public BulkUserDtoMap()
        {
            Map(m => m.FullName).Name("FullName");
            Map(m => m.Email).Name("Email");
            Map(m => m.RoleId).Name("RoleId");
            Map(m => m.ManagerId).Name("ManagerId");
            Map(m => m.IsActive).Name("IsActive");

            // Ignore system field
            Map(m => m.RowNumber).Ignore();
        }
    }
}