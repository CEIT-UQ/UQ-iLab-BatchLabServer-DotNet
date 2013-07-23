using System;
using Library.Lab;
using Library.LabEquipment.Engine;

namespace Library.LabEquipment
{
    public class ExperimentSpecification : LabExperimentSpecification
    {
        #region Constants
        private const string STR_ClassName = "ExperimentSpecification";
        private const Logfile.Level logLevel = Logfile.Level.Fine;
        #endregion

        //---------------------------------------------------------------------------------------//

        public ExperimentSpecification(string xmlSpecification)
            : base(xmlSpecification)
        {
            const string methodName = "ExperimentSpecification";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Nothing to do here
             */

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }
    }
}
