using System;

namespace TypeGuesser
{
    /// <summary>
    /// Thrown when passing both strongly typed objects (e.g. 12.0f) and untyped strings (e.g. "12.0") to a single <see cref="Guesser"/>.  Input to a
    /// guesser must be of a consistent format (either all typed or all untyped).
    /// </summary>
    public class MixedTypingException:Exception
    {
        /// <summary>
        /// Creates a new instance with the given message and inner Exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public MixedTypingException(string message, Exception ex):base(message,ex)
        {
            
        }

        /// <summary>
        /// Creates a new instance with the given message
        /// </summary>
        /// <param name="message"></param>
        public MixedTypingException(string message) : base(message)
        {
            
        }

    }
}
