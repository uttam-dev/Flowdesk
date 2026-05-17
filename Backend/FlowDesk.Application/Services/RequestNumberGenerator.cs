using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Services
{
    public static class RequestNumberGenerator
    {
        public static string Generate(string prefix = "REQ")
        {
            var timestamp = DateTime.UtcNow.ToString("yyMMddHHmm");
            var random = Random.Shared.Next(1000, 9999);

            return $"{prefix}-{timestamp}-{random}";
        }
    }
}
