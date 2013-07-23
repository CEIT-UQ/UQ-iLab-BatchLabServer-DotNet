using System;
using System.Xml.Serialization;

namespace Library.Lab.Types
{
    [Serializable()]
    [XmlType(Namespace = "http://ilab.mit.edu")]
    public class LabExperimentStatus
    {
        private ExperimentStatus experimentStatus;
        private double minTimeToLive;

        //-------------------------------------------------------------------------------------------------//

        [XmlElement("statusReport")]
        public ExperimentStatus ExperimentStatus
        {
            get { return experimentStatus; }
            set { experimentStatus = value; }
        }

        /// <summary>
        /// Guaranteed minimum remaining time (in seconds) before this experiment Id and
        /// associated data will be purged from the LabServer.
        /// </summary>
        [XmlElement("minTimetoLive")]
        public double MinTimeToLive
        {
            get { return minTimeToLive; }
            set { minTimeToLive = value; }
        }

        //-------------------------------------------------------------------------------------------------//

        public LabExperimentStatus()
        {
            this.experimentStatus = new ExperimentStatus();
        }

        public LabExperimentStatus(ExperimentStatus experimentStatus)
        {
            this.experimentStatus = experimentStatus;
        }

        public LabExperimentStatus(ExperimentStatus experimentStatus, double minTimeToLive)
        {
            this.experimentStatus = experimentStatus;
            this.minTimeToLive = minTimeToLive;
        }
    }
}
