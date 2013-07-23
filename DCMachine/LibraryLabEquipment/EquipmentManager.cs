using System;
using Library.Lab;
using Library.LabEquipment.Engine;

namespace Library.LabEquipment
{
    public class EquipmentManager : LabEquipmentManager
    {
        #region Constants
        private const string STR_ClassName = "EquipmentManager";
        private const Logfile.Level logLevel = Logfile.Level.Info;
        /*
         * String constants for logfile messages
         */
        #endregion

        #region Variables
        #endregion

        //-------------------------------------------------------------------------------------------------//

        public EquipmentManager(ConfigProperties configProperties)
            : base(configProperties)
        {
            const string methodName = "EquipmentManager";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Nothing to do here
             */

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        public override bool Create()
        {
            const string methodName = "Create";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Create an instance of the equipment engine
                 */
                this.labEquipmentEngine = new EquipmentEngine(this.labEquipmentConfiguration);
                if (this.labEquipmentEngine == null)
                {
                    throw new ArgumentNullException(EquipmentEngine.ClassName);
                }

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(STR_ClassName, methodName, ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return success;
        }

    }
}
