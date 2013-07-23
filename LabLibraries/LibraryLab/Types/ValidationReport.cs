using System;
using System.Xml.Serialization;

namespace Library.Lab.Types
{
    [Serializable()]
    [XmlType(Namespace = "http://ilab.mit.edu")]
    public class ValidationReport
    {
        private bool accepted;
        private String[] warningMessages;
        private String errorMessage;
        private double estimatedRuntime;

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// True if the experiment specification is acceptable for execution.
        /// </summary>
        [XmlElement("accepted")]
        public bool Accepted
        {
            get { return accepted; }
            set { accepted = value; }
        }

        /// <summary>
        /// Domain-dependent human-readable text containing non-fatal warnings about the experiment.
        /// </summary>
        [XmlArray("warningMessages")]
        public String[] WarningMessages
        {
            get { return warningMessages; }
            set { warningMessages = value; }
        }

        /// <summary>
        /// Domain-dependent human-readable text describing why the experiment specification would not be accepted (if accepted == false).
        /// </summary>
        [XmlElement("errorMessage")]
        public String ErrorMessage
        {
            get { return errorMessage; }
            set { errorMessage = value; }
        }

        /// <summary>
        /// Estimated runtime (in seconds) of this experiment. [OPTIONAL, &lt; 0 if not supported].
        /// </summary>
        [XmlElement("estRuntime")]
        public double EstimatedRuntime
        {
            get { return estimatedRuntime; }
            set { estimatedRuntime = value; }
        }

        //-------------------------------------------------------------------------------------------------//

        public ValidationReport()
        {
        }

        public ValidationReport(bool accepted, double estimatedRuntime)
        {
            this.accepted = accepted;
            this.estimatedRuntime = estimatedRuntime;
        }

        public ValidationReport(string errorMessage)
        {
            this.accepted = false;
            this.estimatedRuntime = -1.0;
            this.errorMessage = errorMessage;
        }
    }
}
