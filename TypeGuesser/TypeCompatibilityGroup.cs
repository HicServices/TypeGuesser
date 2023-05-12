namespace TypeGuesser;

/// <summary>
/// Describes the translation capability of an <see cref="Numerical"/> when assigning a proper C# Type to a collection of input strings.  For example "1" might be picked as a
/// bool but then we see "2" and we can change our minds and assign the collection the Type int (<see cref="TypeCompatibilityGroup"/>).  However if we saw a string "00:00" and
/// decided it meant midnight (TimeSpan) we couldn't then change our minds to DateTime if we saw "2001-01-01 00:00:00" that's not going to fly.
/// </summary>
public enum TypeCompatibilityGroup
{
    /// <summary>
    /// Compatibility group has not been set, i.e. null
    /// </summary>
    None,

    /// <summary>
    /// The Type cannot be converted sensible to another Type e.g. Dates can't be converted to int/decimal etc
    /// </summary>
    Exclusive,

    /// <summary>
    /// Type can be converted to other numerical Types e.g. int values can be converted sensibly to decimal values
    /// </summary>
    Numerical
}