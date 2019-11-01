using System;
using System.Collections.Generic;
using System.Text;

namespace TypeGuesser
{
    /// <summary>
    /// Controls guess decisions where the choice is ambiguous e.g. is "T" and "F" True / False or just a string.  Use
    /// <see cref="GuessSettingsFactory"/> to create instances
    /// </summary>
    public class GuessSettings
    {
        /// <summary>
        /// True if single letter characters (e.g. "T"/"F" or "J/N" or "Y"/"N") should be interpreted as True/False
        /// </summary>
        public bool CharCanBeBoolean { get; set; } = true;

        /// <summary>
        /// Creates a shallow clone of the settings
        /// </summary>
        /// <returns></returns>
        public GuessSettings Clone()
        {
            return (GuessSettings)MemberwiseClone();
        }

        /// <summary>
        /// Copies all values of this object into <paramref name="copyInto"/>
        /// </summary>
        /// <param name="copyInto">The instance to populate with the current values of this</param>
        public void CopyTo(GuessSettings copyInto)
        {
            copyInto.CharCanBeBoolean = CharCanBeBoolean;
        }

        internal GuessSettings()
        {
            
        }
    }
}
