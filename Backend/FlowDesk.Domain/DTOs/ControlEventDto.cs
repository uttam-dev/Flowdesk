using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Domain.DTOs
{
    public class ControlEventDto
    {
        /// <summary>mousemove | mouseclick | keypress | scroll</summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>Relative X coordinate (0.0 – 1.0) of the remote screen.</summary>
        public double X { get; set; }

        /// <summary>Relative Y coordinate (0.0 – 1.0) of the remote screen.</summary>
        public double Y { get; set; }

        /// <summary>Mouse button: "left" | "right"</summary>
        public string? Button { get; set; }

        public bool IsDoubleClick { get; set; }

        /// <summary>Key identifier for keyboard events (e.g. "enter", "a", "backspace").</summary>
        public string? Key { get; set; }

        /// <summary>Modifier keys: "ctrl" | "alt" | "shift" | "command"</summary>
        public string? Modifier { get; set; }

        /// <summary>Scroll direction: "up" | "down"</summary>
        public string? ScrollDirection { get; set; }
    }
}
