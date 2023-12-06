using System;

namespace TypeGuesser;

/// <summary>
/// Thrown when a given Type is not supported by TypeGuesser
/// </summary>
internal class TypeNotSupportedException :Exception
{
    public TypeNotSupportedException(Type t):base(t.FullName)
    {

    }
}