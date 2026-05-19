using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FlowDesk.Domain.Entities
{
    public class EscalationHistory
    {
        public int EscalationHistoryId { get; set; }
        public int RequestId { get; set; }
        public Request? Request { get; set; }

        public int EscalatedBy { get; set; }
        public User? EscalatedByUser { get; set; }

        public DateTime EscalatedOn { get; set; }
        public string? EscalationReason { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}

