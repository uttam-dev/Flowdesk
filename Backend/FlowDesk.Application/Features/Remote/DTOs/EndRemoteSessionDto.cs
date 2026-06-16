using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Features.Remote.DTOs
{
    public class EndRemoteSessionDto
    {
        public string? ResolutionNotes { get; set; }
        public bool ResolveRequest { get; set; }
    }
}
