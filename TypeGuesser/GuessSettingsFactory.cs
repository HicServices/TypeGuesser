namespace TypeGuesser
{
    /// <summary>
    /// Factory for creating instances of <see cref="GuessSettings"/>.  Contains a static variable for the defaults
    /// </summary>
    public class GuessSettingsFactory
    {
        /// <summary>
        /// The system wide default values for flags in <see cref="GuessSettings"/>
        /// </summary>
        public static GuessSettings Defaults = new GuessSettings();
        
        /// <summary>
        /// Creates a new instance of <see cref="GuessSettings"/> using the currently configured defaults in the static
        /// variable <see cref="Defaults"/>
        /// </summary>
        /// <returns></returns>
        public GuessSettings Create()
        {
            return Defaults.Clone();
        }
    }
}