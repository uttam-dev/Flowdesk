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

        /// <summary>
        /// The support user currently assigned to this request, or <c>null</c> if unassigned.
        /// Populated by <see cref="IRequestsService.GetRespectiveIds"/> for use in
        /// real-time notification behaviors.
        /// </summary>
        public int? AssignedToId { get; set; }
    }
}
