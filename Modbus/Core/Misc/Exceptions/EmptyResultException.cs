using System;

namespace Core.Misc.Exceptions
{
    public class EmptyResultException : Exception
    {
        public EmptyResultException()
        {
        }

        public EmptyResultException(string message)
            : base(message)
        {
        }

        public EmptyResultException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
