using System;

namespace Library.ServiceBroker.Database.Types
{
    public class ExperimentInfo
    {
        private int experimentId;
        private String labServerGuid;

        public int ExperimentId
        {
            get { return experimentId; }
            set { experimentId = value; }
        }

        public String LabServerGuid
        {
            get { return labServerGuid; }
            set { labServerGuid = value; }
        }

        public ExperimentInfo()
        {
            this.experimentId = -1;
        }

        public ExperimentInfo(String labServerGuid)
        {
            this.experimentId = -1;
            this.labServerGuid = labServerGuid;
        }
    }
}
