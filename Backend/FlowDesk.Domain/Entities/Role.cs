using FlowDesk.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Domain.Entities
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;

        public ICollection<User>? Users { get; set; }
    }
}
