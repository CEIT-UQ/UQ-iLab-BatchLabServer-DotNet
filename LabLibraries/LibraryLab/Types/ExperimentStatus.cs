using System;
using System.Xml.Serialization;

namespace Library.Lab.Types
{
    [Serializable()]
    [XmlType(Namespace = "http://ilab.mit.edu")]
    public class ExperimentStatus
    {
        private StatusCodes statusCode;
        private WaitEstimate waitEstimate;
        private double estimatedRuntime;
        private double estimatedRemainingRuntime;

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Indicates the status of this experiment.
        /// </summary>
        [XmlElement("statusCode")]
        public int IntStatusCode
        {
            get { return (int)statusCode; }
            set { statusCode = (StatusCodes)value; }
        }

        [XmlIgnore]
        public StatusCodes StatusCode
        {
            get { return statusCode; }
            set { statusCode = value; }
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

        /// <summary>
        /// Estimated runtime (in seconds) of this experiment.
        /// [OPTIONAL, &lt; 0 if not used].
        /// </summary>
        [XmlElement("estRuntime")]
        public double EstimatedRuntime
        {
            get { return estimatedRuntime; }
            set { estimatedRuntime = value; }
        }

        /// <summary>
        /// Estimated remaining run time (in seconds) of this experiment, if the experiment is currently running.
        /// [OPTIONAL, &lt; 0 if not used].
        /// </summary>
        [XmlElement("estRemainingRuntime")]
        public double EstimatedRemainingRuntime
        {
            get { return estimatedRemainingRuntime; }
            set { estimatedRemainingRuntime = value; }
        }

        //-------------------------------------------------------------------------------------------------//

        public ExperimentStatus()
        {
            this.statusCode = StatusCodes.Unknown;
            this.waitEstimate = new WaitEstimate();
        }

        public ExperimentStatus(StatusCodes statusCode)
        {
            this.statusCode = statusCode;
            this.waitEstimate = new WaitEstimate();
        }

        public ExperimentStatus(StatusCodes statusCode, WaitEstimate waitEstimate, double estimatedRuntime, double estimatedRemainingRuntime)
        {
            this.statusCode = statusCode;
            this.waitEstimate = waitEstimate;
            this.estimatedRuntime = estimatedRuntime;
            this.estimatedRemainingRuntime = estimatedRemainingRuntime;
        }
    }
}
