using System;
using System.Xml.Serialization;

namespace Library.Lab.Types
{
    [Serializable()]
    [XmlType(Namespace = "http://ilab.uq.edu.au/")]
    public class Validation
    {
        private bool accepted;
        private int executionTime;
        private String errorMessage;

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// True if vaildation successful.
        /// </summary>
        [XmlElement("Accepted")]
        public bool Accepted
        {
            get { return this.accepted; }
            set { this.accepted = value; }
        }

        /// <summary>
        /// Execution time (in seconds) if validation successful.
        /// </summary>
        [XmlElement("ExecutionTime")]
        public int ExecutionTime
        {
            get { return this.executionTime; }
            set { this.executionTime = value; }
        }

        /// <summary>
        /// Error message if validation unsuccessful.
        /// </summary>
        [XmlElement("ErrorMessage")]
        public String ErrorMessage
        {
            get { return this.errorMessage; }
            set { this.errorMessage = value; }
        }

        //-------------------------------------------------------------------------------------------------//

        public Validation()
        {
            this.executionTime = -1;
        }

        public Validation(bool accepted, int executionTime)
        {
            this.accepted = accepted;
            this.executionTime = executionTime;
        }

        public Validation(string errorMessage)
        {
            this.accepted = false;
            this.executionTime = -1;
            this.errorMessage = errorMessage;
        }
    }
}
