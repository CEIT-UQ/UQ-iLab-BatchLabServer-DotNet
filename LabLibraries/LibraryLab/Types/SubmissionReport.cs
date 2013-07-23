using System;
using System.Xml.Serialization;

namespace Library.Lab.Types
{
    [Serializable()]
    [XmlType(Namespace = "http://ilab.mit.edu")]
    public class SubmissionReport
    {
        private int experimentId;
        private ValidationReport validationReport;
        private WaitEstimate waitEstimate;
        private double minTimeToLive;

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        [XmlElement("wait")]
        public WaitEstimate WaitEstimate
        {
            get { return waitEstimate; }
            set { waitEstimate = value; }
        }

        //-------------------------------------------------------------------------------------------------//

        public SubmissionReport()
        {
            this.experimentId = -1;
            this.validationReport = new ValidationReport();
            this.waitEstimate = new WaitEstimate();
        }

        public SubmissionReport(int experimentId)
        {
            this.experimentId = experimentId;
            this.validationReport = new ValidationReport();
            this.waitEstimate = new WaitEstimate();
        }

        public SubmissionReport(int experimentId, ValidationReport validationReport, WaitEstimate waitEstimate, double minTimeToLive)
        {
            this.validationReport = validationReport;
            this.experimentId = experimentId;
            this.minTimeToLive = minTimeToLive;
            this.waitEstimate = waitEstimate;
        }
    }
}
