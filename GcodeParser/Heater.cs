using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GcodeParser
{
    /// <summary>
    /// Enum for different heating elements
    /// </summary>
    public enum Heater
    {
        /// <summary>
        /// Heater for bed
        /// </summary>
        bed,
        /// <summary>
        /// Heater for chamber
        /// </summary>
        chamber,
        /// <summary>
        /// Heater for hot end
        /// </summary>
        hotend,
    }
}
