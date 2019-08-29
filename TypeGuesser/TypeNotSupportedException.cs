using System;
using System.Collections.Generic;
using System.Text;

namespace TypeGuesser
{
    /// <summary>
    /// Thrown when a given Type is not supported by TypeGuesser
    /// </summary>
    class TypeNotSupportedException :Exception
    {
        public TypeNotSupportedException(Type t):base(t.FullName)
        {
            
        }
    }
}
