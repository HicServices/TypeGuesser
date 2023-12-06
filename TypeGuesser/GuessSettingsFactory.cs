namespace TypeGuesser;

/// <summary>
/// Factory for creating instances of <see cref="GuessSettings"/>.  Contains a static variable for the defaults
/// </summary>
public static class GuessSettingsFactory
{
    /// <summary>
    /// The system wide default values for flags in <see cref="GuessSettings"/>
    /// </summary>
    public static readonly GuessSettings Defaults = new();

    /// <summary>
    /// Creates a new instance of <see cref="GuessSettings"/> using the currently configured defaults in the static
    /// variable <see cref="Defaults"/>
    /// </summary>
    /// <returns></returns>
    public static GuessSettings Create()
    {
        return Defaults.Clone();
    }
}