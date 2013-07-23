using System;
using System.Xml.Serialization;

namespace Library.Lab.Types
{
    [Serializable()]
    [XmlType(Namespace = "http://ilab.mit.edu")]
    public class WaitEstimate
    {
        private int effectiveQueueLength;
        private double estimatedWait;

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Number of experiments currently in the execution queue that would run before the hypothetical new experiment.
        /// </summary>
        [XmlElement("effectiveQueueLength")]
        public int EffectiveQueueLength
        {
            get { return effectiveQueueLength; }
            set { effectiveQueueLength = value; }
        }

        /// <summary>
        /// Estimated wait time (in seconds) until the hypothetical new experiment would begin,
        /// based on the other experiments currently in the execution queue.
        /// [OPTIONAL, &lt; 0 if not supported].
        /// </summary>
        [XmlElement("estWait")]
        public double EstimatedWait
        {
            get { return estimatedWait; }
            set { estimatedWait = value; }
        }

        //-------------------------------------------------------------------------------------------------//

        public WaitEstimate()
        {
        }

        public WaitEstimate(int effectiveQueueLength, double estimatedWait)
        {
            this.effectiveQueueLength = effectiveQueueLength;
            this.estimatedWait = estimatedWait;
        }
    }
}
