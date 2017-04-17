using System;

namespace Core.Misc.Exceptions
{
    public class InvalidSettingsException : Exception
    {
        public InvalidSettingsException()
        {
        }

        public InvalidSettingsException(string message)
        : base(message)
        {
        }

        public InvalidSettingsException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}
