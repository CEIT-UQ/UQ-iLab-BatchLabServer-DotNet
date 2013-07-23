using System;
using System.Xml.Serialization;

namespace Library.Lab.Types
{
    [Serializable]
    [XmlType(Namespace = "http://ilab.uq.edu.au/")]
    public class ExecutionStatus
    {
        public enum Status
        {
            None = 0,
            Created = 1,
            Initialising = 2,
            Starting = 3,
            Running = 4,
            Stopping = 5,
            Finalising = 6,
            Done = 7,
            Completed = 8,
            Failed = 9,
            Cancelled = 10
        }

        //-------------------------------------------------------------------------------------------------//

        private int executionId;
        private Status executeStatus;
        private Status resultStatus;
        private int timeRemaining;
        private String errorMessage;

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// The identity of the currently executing driver
        /// </summary>
        public int ExecutionId
        {
            get { return this.executionId; }
            set { this.executionId = value; }
        }

        /// <summary>
        /// Status of the currently executing driver converted to an int value for passing through the web service
        /// </summary>
        [XmlElement("ExecuteStatus")]
        public int IntExecuteStatus
        {
            get { return (int)this.executeStatus; }
            set { this.executeStatus = (Status)value; }
        }

        [XmlIgnore]
        public Status ExecuteStatus
        {
            get { return this.executeStatus; }
            set { this.executeStatus = value; }
        }

        /// <summary>
        /// Result status of the most recent driver execution converted to an int value for passing through the web service
        /// </summary>
        [XmlElement("ResultStatus")]
        public int IntResultStatus
        {
            get { return (int)this.resultStatus; }
            set { this.resultStatus = (Status)value; }
        }

        [XmlIgnore]
        public Status ResultStatus
        {
            get { return this.resultStatus; }
            set { this.resultStatus = value; }
        }

        /// <summary>
        /// Time remaining (in seconds) for the currently executing driver
        /// </summary>
        [XmlElement("TimeRemaining")]
        public int TimeRemaining
        {
            get { return this.timeRemaining; }
            set { this.timeRemaining = value; }
        }

        /// <summary>
        /// Information about driver execution that did not complete successfully
        /// </summary>
        [XmlElement("ErrorMessage")]
        public String ErrorMessage
        {
            get { return this.errorMessage; }
            set { this.errorMessage = value; }
        }

        //-------------------------------------------------------------------------------------------------//

        public ExecutionStatus()
        {
            this.executeStatus = Status.None;
            this.resultStatus = Status.None;
            this.timeRemaining = -1;
            this.errorMessage = String.Empty;
        }
    }
}
