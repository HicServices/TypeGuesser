using System;

namespace TypeGuesser;

/// <summary>
/// Thrown when a given Type is not supported by TypeGuesser
/// </summary>
internal sealed class TypeNotSupportedException(Type t) : Exception(t.FullName);