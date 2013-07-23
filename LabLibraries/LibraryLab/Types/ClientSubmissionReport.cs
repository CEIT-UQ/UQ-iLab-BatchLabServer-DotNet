using System;
using System.Xml.Serialization;

namespace Library.Lab.Types
{
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://ilab.mit.edu")]
    public class ClientSubmissionReport
    {
        private ValidationReport validationReport;
        private int experimentId;
        private double minTimeToLive;
        private WaitEstimate waitEstimate;

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// A copy of the ValidationReport returned by the LabServer
        /// </summary>
        [XmlElement("vReport")]
        public ValidationReport ValidationReport
        {
            get { return validationReport; }
            set { validationReport = value; }
        }

        /// <summary>
        /// A number greater than zero that identifies the experiment.
        /// </summary>
        [XmlElement("experimentID")]
        public int ExperimentId
        {
            get { return experimentId; }
            set { experimentId = value; }
        }

        /// <summary>
        /// Guaranteed minimum time (in seconds, starting now) before this experiment Id and associated data
        /// will be purged from the LabServer.
        /// </summary>
        [XmlElement("minTimeToLive")]
        public double MinTimeToLive
        {
            get { return minTimeToLive; }
            set { minTimeToLive = value; }
        }

        /// <summary>
        /// An instance of a WaitEstimate class containing the estimated wait time before this experiment will execute.
        /// </summary>
        [XmlElement("wait")]
        public WaitEstimate WaitEstimate
        {
            get { return waitEstimate; }
            set { waitEstimate = value; }
        }

        //-------------------------------------------------------------------------------------------------//

        public ClientSubmissionReport()
        {
            this.experimentId = -1;
            this.validationReport = new ValidationReport();
            this.waitEstimate = new WaitEstimate();
        }

        public ClientSubmissionReport(int experimentId)
        {
            this.experimentId = experimentId;
            this.validationReport = new ValidationReport();
            this.waitEstimate = new WaitEstimate();
        }

        public ClientSubmissionReport(int experimentId, ValidationReport validationReport, WaitEstimate waitEstimate, double minTimeToLive)
        {
            this.validationReport = validationReport;
            this.experimentId = experimentId;
            this.minTimeToLive = minTimeToLive;
            this.waitEstimate = waitEstimate;
        }
    }
}
