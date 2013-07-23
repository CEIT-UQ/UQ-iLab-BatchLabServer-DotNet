using System;

namespace Library.Lab.Exceptions
{
    public class ProtocolException : Exception
    {
        public ProtocolException()
            : base()
        {
        }

        public ProtocolException(string message)
            : base(message)
        {
        }

        public ProtocolException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
