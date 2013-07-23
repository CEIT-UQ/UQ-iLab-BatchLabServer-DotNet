using System;

namespace Library.Lab.Exceptions
{
    public class XmlUtilitiesException : Exception
    {
        public XmlUtilitiesException()
            : base()
        {
        }

        public XmlUtilitiesException(string message)
            : base(message)
        {
        }

        public XmlUtilitiesException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
