using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Domain.DTOs
{
    public class RequestRespectiveIdResponseDto
    {
        public int RequestId { get; set; }
        public int EmployeeId { get; set; }
        public int? ManagerId { get; set; }
    }
}
