using CsvHelper;
using CsvHelper.Configuration;
using FlowDesk.Application.Common.Interfaces;
using FlowDesk.Application.Features.Users.DTOs;
using FlowDesk.Infrastructure.Files.Mapping;
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

        // ================= CSV =================
        private List<BulkUserDto> ParseCsv(Stream stream)
        {
            using var reader = new StreamReader(stream);

            var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null,
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim
            };

            using var csv = new CsvReader(reader, config);

            csv.Context.RegisterClassMap<BulkUserDtoMap>();

            var records = new List<BulkUserDto>();
            int rowNumber = 2;

            while (csv.Read())
            {
                try
                {
                    var dto = csv.GetRecord<BulkUserDto>();

                    dto.RowNumber = rowNumber++;

                    // Normalize safely
                    dto.FullName = SafeGet(dto.FullName);
                    dto.Email = SafeGet(dto.Email)?.ToLower();
                    dto.RoleName = SafeGet(dto.RoleName);
                    dto.ManagerEmail = SafeGet(dto.ManagerEmail)?.ToLower();

                    // Handle bool safely (important for CSV corruption)
                    dto.IsActive = dto.IsActive ?? true;

                    // Skip completely empty rows
                    if (IsRowEmpty(dto))
                        continue;

                    records.Add(dto);
                }
                catch
                {
                    // Skip broken row but continue processing
                    rowNumber++;
                    continue;
                }
            }

            return records;
        }

        // ================= EXCEL =================
        private List<BulkUserDto> ParseExcel(Stream stream)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
            var sheet = workbook.Worksheet(1);

            var rows = sheet?.RangeUsed()?.RowsUsed().Skip(1);

            var list = new List<BulkUserDto>();
            int rowNum = 2;

            foreach (var row in rows!)
            {
                try
                {
                    var dto = new BulkUserDto
                    {
                        RowNumber = rowNum++,

                        FullName = SafeGet(row.Cell(1).GetString()),
                        Email = SafeGet(row.Cell(2).GetString())?.ToLower(),

                        RoleName = SafeGet(row.Cell(3).GetString()),
                        ManagerEmail = SafeGet(row.Cell(4).GetString())?.ToLower(),

                        // Safe bool handling
                        IsActive = row.Cell(5).TryGetValue<bool>(out var val)
                            ? val
                            : true
                    };

                    // Skip empty rows
                    if (IsRowEmpty(dto))
                        continue;

                    list.Add(dto);
                }
                catch
                {
                    // Skip invalid row safely
                    rowNum++;
                    continue;
                }
            }

            return list;
        }

        // ================= NORMALIZATION =================
        private void Normalize(BulkUserDto dto)
        {
            dto.FullName = dto.FullName?.Trim()!;
            dto.Email = dto.Email?.Trim().ToLower()!;
            dto.RoleName = dto.RoleName?.Trim()!;
            dto.ManagerEmail = dto.ManagerEmail?.Trim().ToLower();
        }

        private static string? SafeGet(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static bool? SafeBool(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            return value.Trim().ToLower() switch
            {
                "true" => true,
                "false" => false,
                _ => null
            };
        }

        private static bool IsRowEmpty(BulkUserDto dto)
        {
            return string.IsNullOrWhiteSpace(dto.FullName) &&
                   string.IsNullOrWhiteSpace(dto.Email) &&
                   string.IsNullOrWhiteSpace(dto.RoleName) &&
                   string.IsNullOrWhiteSpace(dto.ManagerEmail) &&
                   dto.IsActive == null;
        }
    }
}