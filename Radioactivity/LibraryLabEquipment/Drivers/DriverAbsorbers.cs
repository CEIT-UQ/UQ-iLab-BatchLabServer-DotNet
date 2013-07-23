using System;
using Library.Lab;
using Library.Lab.Types;
using Library.LabEquipment.Engine;
using Library.LabEquipment.Types;
using Library.LabEquipment.Devices;
using System.Diagnostics;

namespace Library.LabEquipment.Drivers
{
    public class DriverAbsorbers : DriverEquipment
    {
        #region Constants
        private const String STR_ClassName = "DriverAbsorbers";
        private const Logfile.Level logLevel = Logfile.Level.Finer;
        #endregion

        #region Properties

        public new static String ClassName
        {
            get { return STR_ClassName; }
        }

        public override String DriverName
        {
            get { return STR_ClassName; }
        }

        #endregion

        #region Types
        private enum States
        {
            SelectSourceMessageLine1, SelectSourceMessageLine2, SelectSource,
            SetTubeDistanceMessageLine1, SetTubeDistanceMessageLine2, SetTubeDistance,
            SelectAbsorberMessageLine1, SelectAbsorberMessageLine2, SelectAbsorber,
            AllocateCaptureData, CaptureDataMessageLine1, CaptureDataMessageLine2, CaptureData,
            ReturnSourceMessageLine1, ReturnSourceMessageLine2, ReturnSource,
            ReturnAbsorberMessageLine1, ReturnAbsorberMessageLine2, ReturnAbsorber,
            ReturnTubeMessageLine1, ReturnTubeMessageLine2, ReturnTube,
            ReturnToReadyMessageLine1, ReturnToReadyMessageLine2,
            Completed, Done
        }
        #endregion

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="labEquipmentConfiguration"></param>
        public DriverAbsorbers(LabEquipmentConfiguration labEquipmentConfiguration)
            : base(labEquipmentConfiguration)
        {
            const String methodName = "DriverAbsorbers";
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
        /// <param name="xmlSpecification"></param>
        /// <returns>Validation</returns>
        public override Validation Validate(String xmlSpecification)
        {
            const String methodName = "Validate";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            Validation validation;

            try
            {
                /*
                 * Check that parameters are valid
                 */
                base.Validate(xmlSpecification);

                /*
                 * Check that the equipment does indeed have absorbers
                 */
                if (this.deviceFlexMotion.AbsorbersPresent == false)
                {
                    throw new ApplicationException(STRERR_EquipmentHasNoAbsorbers);
                }

                /*
                 * Check the setup Id
                 */
                String setupId = this.experimentSpecification.SetupId;
                switch (setupId)
                {
                    case Consts.STRXML_SetupId_RadioactivityVsAbsorber:
                        /*
                         * Check that the correct devices have been set
                         */
                        //if (this.deviceFlexMotion.GetType().Equals(typeof(DeviceFlexMotion)) == false)
                        //{
                        //    throw new NullReferenceException(String.Format(STRERR_InvalidDeviceInstance_arg, DeviceFlexMotion.ClassName));
                        //}
                        //if (this.deviceST360Counter.GetType().Equals(typeof(DeviceST360Counter)) == false)
                        //{
                        //    throw new NullReferenceException(String.Format(STRERR_InvalidDeviceInstance_arg, DeviceST360Counter.ClassName));
                        //}
                        //if (this.deviceSerialLcd.GetType().Equals(typeof(DeviceSerialLcd)) == false)
                        //{
                        //    throw new NullReferenceException(String.Format(STRERR_InvalidDeviceInstance_arg, DeviceSerialLcd.ClassName));
                        //}

                        /*
                         * Specification is valid
                         */
                        validation = new Validation(true, this.GetExecutionTime(experimentSpecification));
                        break;

                    default:
                        /*
                         * Don't throw an exception, a derived class will want to check the setup Id
                         */
                        validation = new Validation(String.Format(STRERR_InvalidSetupId_arg, setupId));
                        break;
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                validation = new Validation(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Validation_arg3,
                    validation.Accepted, validation.ExecutionTime, validation.ErrorMessage));

            return validation;
        }

        //-------------------------------------------------------------------------------------------------//

        protected override int GetExecutionTime(ExperimentSpecification experimentSpecification)
        {
            const String methodName = "GetExecutionTime";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            double executionTime;

            /*
             * No initialising
             */
            this.executionTimes.Initialise = 0;

            /*
             * Get source select time
             */
            executionTime = this.deviceSerialLcd.GetWriteLineTime() * 2;
            executionTime += this.deviceFlexMotion.GetSourceSelectTime(experimentSpecification.SourceName);

            /*
             * Get tube move time
             */
            executionTime += this.deviceSerialLcd.GetWriteLineTime() * 2;
            executionTime += this.deviceFlexMotion.GetTubeMoveTime(this.deviceFlexMotion.TubeDistanceHome, experimentSpecification.Distances[0]);

            /*
             * Set starting time
             */
            this.executionTimes.Start = (int)(executionTime + 0.5);

            /*
             * Calculate running time
             */
            String[] absorbers = experimentSpecification.AbsorberNames;
            double selectTimeHome = this.deviceFlexMotion.GetAbsorberSelectTime(this.deviceFlexMotion.AbsorberNameHome);
            double lastReturnTime = this.deviceFlexMotion.GetAbsorberReturnTime(this.deviceFlexMotion.AbsorberNameHome);
            executionTime = 0.0;
            for (int i = 0; i < absorbers.Length; i++)
            {
                /*
                 * Get absorber select time
                 */
                executionTime += this.deviceSerialLcd.GetWriteLineTime() * 2;
                double selectTime = this.deviceFlexMotion.GetAbsorberSelectTime(absorbers[i]);
                double returnTime = this.deviceFlexMotion.GetAbsorberReturnTime(absorbers[i]);

                if (i > 0)
                {
                    /*
                     * Get time to move to the next absorber - calculated as follows:
                     * B -> C = ReturnTime(B) + (SelectTime(C) - SelectTime(Home))
                     */
                    selectTime = lastReturnTime + selectTime - selectTimeHome;
                }
                executionTime += selectTime;

                /*
                 * Get capture data time
                 */
                executionTime += this.deviceSerialLcd.GetWriteLineTime() * 2;
                executionTime += this.deviceST360Counter.GetCaptureDataTime(experimentSpecification.Duration) * experimentSpecification.Repeat;

                /*
                 * Save absorber return time for next iteration
                 */
                lastReturnTime = returnTime;
            }

            /*
             * Set running time
             */
            this.executionTimes.Run = (int)(executionTime + 0.5);

            /*
             * Get source return time
             */
            executionTime = this.deviceSerialLcd.GetWriteLineTime() * 2;
            executionTime += this.deviceFlexMotion.GetSourceReturnTime(experimentSpecification.SourceName);

            /*
             * Get absorber return time
             */
            executionTime += this.deviceSerialLcd.GetWriteLineTime() * 2;
            executionTime += this.deviceFlexMotion.GetAbsorberReturnTime(absorbers[absorbers.Length - 1]);

            /*
             * Get tube return time
             */
            executionTime += this.deviceSerialLcd.GetWriteLineTime() * 2;
            executionTime += this.deviceFlexMotion.GetTubeMoveTime(experimentSpecification.Distances[0], this.deviceFlexMotion.TubeDistanceHome);

            /*
             * Set stopping time
             */
            this.executionTimes.Stop = (int)(executionTime + 0.5);

            /*
             * No finalising
             */
            this.executionTimes.Finalise = 0;

            /*
             * Get the total execution time
             */
            int totalExecutionTime = this.executionTimes.TotalExecutionTime;

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_ExecutionTime_arg, totalExecutionTime));

            return totalExecutionTime;
        }

        //-------------------------------------------------------------------------------------------------//

        protected override bool ExecuteStarting()
        {
            const String methodName = "ExecuteStarting";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Initialise local variables
                 */
                String sourceName = this.experimentSpecification.SourceName;
                int distance = this.experimentSpecification.Distances[0];
                String lcdMessage = null;

                /*
                 * Initialise state machine
                 */
                States thisState = States.SelectSourceMessageLine1;

                /*
                 * State machine loop
                 */
                while (thisState != States.Done)
                {
                    switch (thisState)
                    {
                        case States.SelectSourceMessageLine1:
                            if (this.deviceSerialLcd.WriteLine(DeviceSerialLcd.LineNumber.One, STRLCD_SelectSource) == false)
                            {
                                throw new ApplicationException(this.deviceSerialLcd.LastError);
                            }

                            thisState = States.SelectSourceMessageLine2;
                            break;

                        case States.SelectSourceMessageLine2:
                            if (this.deviceSerialLcd.WriteLine(DeviceSerialLcd.LineNumber.Two, sourceName) == false)
                            {
                                throw new ApplicationException(this.deviceSerialLcd.LastError);
                            }

                            thisState = States.SelectSource;
                            break;

                        case States.SelectSource:
                            if (this.deviceFlexMotion.SelectSource(sourceName) == false)
                            {
                                throw new ApplicationException(this.deviceFlexMotion.LastError);
                            }

                            thisState = States.SetTubeDistanceMessageLine1;
                            break;

                        case States.SetTubeDistanceMessageLine1:
                            if (this.deviceSerialLcd.WriteLine(DeviceSerialLcd.LineNumber.One, STRLCD_SetDistance) == false)
                            {
                                throw new ApplicationException(this.deviceSerialLcd.LastError);
                            }

                            thisState = States.SetTubeDistanceMessageLine2;
                            break;

                        case States.SetTubeDistanceMessageLine2:
                            lcdMessage = String.Format(STRLCD_Distance_arg, distance);
                            if (this.deviceSerialLcd.WriteLine(DeviceSerialLcd.LineNumber.Two, lcdMessage) == false)
                            {
                                throw new ApplicationException(this.deviceSerialLcd.LastError);
                            }

                            thisState = States.SetTubeDistance;
                            break;

                        case States.SetTubeDistance:
                            if (this.deviceFlexMotion.SetTubeDistance(distance) == false)
                            {
                                throw new ApplicationException(this.deviceFlexMotion.LastError);
                            }

                            thisState = States.Done;
                            break;

                    }

                    /*
                     * Check if the experiment has been cancelled
                     */
                    if (this.cancelled == true)
                    {
                        thisState = States.Done;
                    }
                }

                success = (this.cancelled == false);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                this.executionStatus.ErrorMessage = ex.Message;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        protected override bool ExecuteRunning()
        {
            const String methodName = "ExecuteRunning";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Initialise local variables
                 */
                this.experimentResults.DataVectors = new int[this.experimentSpecification.AbsorberNames.Length][];
                int absorberIndex = 0;
                int repeatIndex = 0;
                int distance = this.experimentSpecification.Distances[0];
                int duration = 0;
                String absorberName = null;
                String lcdMessage = null;

                /*
                 * Initialise state machine
                 */
                States thisState = States.SelectAbsorberMessageLine1;

                /*
                 * State machine loop
                 */
                while (thisState != States.Done)
                {
                    switch (thisState)
                    {
                        case States.SelectAbsorberMessageLine1:
                            absorberName = this.experimentSpecification.AbsorberNames[absorberIndex];
                            duration = this.experimentSpecification.Duration;

                            if (this.deviceSerialLcd.WriteLine(DeviceSerialLcd.LineNumber.One, STRLCD_SelectAbsorber) == false)
                            {
                                throw new ApplicationException(this.deviceSerialLcd.LastError);
                            }

                            thisState = States.SelectAbsorberMessageLine2;
                            break;

                        case States.SelectAbsorberMessageLine2:
                            if (this.deviceSerialLcd.WriteLine(DeviceSerialLcd.LineNumber.Two, absorberName) == false)
                            {
                                throw new ApplicationException(this.deviceSerialLcd.LastError);
                            }

                            thisState = States.SelectAbsorber;
                            break;

                        case States.SelectAbsorber:
                            if (this.deviceFlexMotion.SelectAbsorber(absorberName) == false)
                            {
                                throw new ApplicationException(this.deviceFlexMotion.LastError);
                            }

                            thisState = States.AllocateCaptureData;
                            break;


                        case States.AllocateCaptureData:
                            /*
                             * Allocate storage for the capture data
                             */
                            this.experimentResults.DataVectors[absorberIndex] = new int[this.experimentSpecification.Repeat];
                            repeatIndex = 0;

                            thisState = States.CaptureDataMessageLine1;
                            break;

                        case States.CaptureDataMessageLine1:
                            if (this.deviceSerialLcd.WriteLine(DeviceSerialLcd.LineNumber.One, STRLCD_CaptureCounts) == false)
                            {
                                throw new ApplicationException(this.deviceSerialLcd.LastError);
                            }

                            thisState = States.CaptureDataMessageLine2;
                            break;

                        case States.CaptureDataMessageLine2:
                            lcdMessage = String.Format(STRLCD_CaptureData_arg4, distance, duration, repeatIndex + 1, experimentSpecification.Repeat);
                            if (this.deviceSerialLcd.WriteLine(DeviceSerialLcd.LineNumber.Two, lcdMessage) == false)
                            {
                                throw new ApplicationException(this.deviceSerialLcd.LastError);
                            }

                            thisState = States.CaptureData;
                            break;

                        case States.CaptureData:
                            /*
                             * Check if device is simulated
                             */
                            if (this.deviceST360Counter.GetType().Equals(typeof(DeviceST360CounterSimulation)) == true)
                            {
                                DeviceST360CounterSimulation deviceST360CounterSimulation = (DeviceST360CounterSimulation)this.deviceST360Counter;

                                /*
                                 * Set the absorption percentage for the specified absorber and the tube distance
                                 */
                                deviceST360CounterSimulation.Absorption = this.deviceFlexMotion.GetAbsorption(absorberName);
                                deviceST360CounterSimulation.Distance = distance;
                            }

                            /*
                             * Capture data for this duration
                             */
                            int counts;
                            if (this.deviceST360Counter.CaptureData(duration, out counts) == false)
                            {
                                throw new ApplicationException(this.deviceST360Counter.LastError);
                            }
                            this.experimentResults.DataVectors[absorberIndex][repeatIndex] = counts;

                            /*
                             * Check if all repeats at this absorber have been processed
                             */
                            if (++repeatIndex == this.experimentSpecification.Repeat)
                            {
                                /*
                                 * Check if all absorbers have been processed
                                 */
                                if (++absorberIndex == this.experimentSpecification.AbsorberNames.Length)
                                {
                                    thisState = States.Done;
                                    break;
                                }

                                thisState = States.SelectAbsorberMessageLine1;
                                break;
                            }

                            thisState = States.CaptureDataMessageLine2;
                            break;
                    }

                    /*
                     * Check if the experiment has been cancelled
                     */
                    if (this.cancelled == true)
                    {
                        thisState = States.Done;
                    }
                }

                if (this.cancelled == false)
                {

                    success = true;
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                this.executionStatus.ErrorMessage = ex.Message;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        protected override bool ExecuteStopping()
        {
            const String methodName = "ExecuteStopping";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Initialise state machine
                 */
                States thisState = States.ReturnSourceMessageLine1;
                String lastError = null;

                /*
                 * State machine loop
                 */
                while (thisState != States.Done)
                {
                    switch (thisState)
                    {
                        case States.ReturnSourceMessageLine1:
                            if (this.deviceSerialLcd.WriteLine(DeviceSerialLcd.LineNumber.One, STRLCD_ReturnSource) == false)
                            {
                                if (lastError == null)
                                {
                                    lastError = this.deviceSerialLcd.LastError;
                                }
                            }

                            thisState = States.ReturnSourceMessageLine2;
                            break;

                        case States.ReturnSourceMessageLine2:
                            if (this.deviceSerialLcd.WriteLine(DeviceSerialLcd.LineNumber.Two, String.Empty) == false)
                            {
                                if (lastError == null)
                                {
                                    lastError = this.deviceSerialLcd.LastError;
                                }
                            }

                            thisState = States.ReturnSource;
                            break;

                        case States.ReturnSource:
                            if (this.deviceFlexMotion.SelectSource(this.deviceFlexMotion.SourceNameHome) == false)
                            {
                                if (lastError == null)
                                {
                                    lastError = this.deviceFlexMotion.LastError;
                                }
                            }

                            thisState = States.ReturnAbsorberMessageLine1;
                            break;

                        case States.ReturnAbsorberMessageLine1:
                            if (this.deviceSerialLcd.WriteLine(DeviceSerialLcd.LineNumber.One, STRLCD_ReturnAbsorber) == false)
                            {
                                if (lastError == null)
                                {
                                    lastError = this.deviceSerialLcd.LastError;
                                }
                            }

                            thisState = States.ReturnAbsorberMessageLine2;
                            break;

                        case States.ReturnAbsorberMessageLine2:
                            if (this.deviceSerialLcd.WriteLine(DeviceSerialLcd.LineNumber.Two, String.Empty) == false)
                            {
                                if (lastError == null)
                                {
                                    lastError = this.deviceSerialLcd.LastError;
                                }
                            }

                            thisState = States.ReturnAbsorber;
                            break;

                        case States.ReturnAbsorber:
                            if (this.deviceFlexMotion.SelectAbsorber(this.deviceFlexMotion.AbsorberNameHome) == false)
                            {
                                if (lastError == null)
                                {
                                    lastError = this.deviceFlexMotion.LastError;
                                }
                            }

                            thisState = States.ReturnTubeMessageLine1;
                            break;

                        case States.ReturnTubeMessageLine1:
                            if (this.deviceSerialLcd.WriteLine(DeviceSerialLcd.LineNumber.One, STRLCD_ReturnTube) == false)
                            {
                                if (lastError == null)
                                {
                                    lastError = this.deviceSerialLcd.LastError;
                                }
                            }

                            thisState = States.ReturnTubeMessageLine2;
                            break;

                        case States.ReturnTubeMessageLine2:
                            if (this.deviceSerialLcd.WriteLine(DeviceSerialLcd.LineNumber.Two, String.Empty) == false)
                            {
                                if (lastError == null)
                                {
                                    lastError = this.deviceSerialLcd.LastError;
                                }
                            }

                            thisState = States.ReturnTube;
                            break;

                        case States.ReturnTube:
                            if (this.deviceFlexMotion.SelectTubeDistance(this.deviceFlexMotion.TubeDistanceHome) == false)
                            {
                                if (lastError == null)
                                {
                                    lastError = this.deviceFlexMotion.LastError;
                                }
                            }

                            thisState = States.ReturnToReadyMessageLine1;
                            break;

                        case States.ReturnToReadyMessageLine1:
                            if (this.deviceSerialLcd.WriteLine(DeviceSerialLcd.LineNumber.One, STRLCD_Ready) == false)
                            {
                                if (lastError == null)
                                {
                                    lastError = this.deviceSerialLcd.LastError;
                                }
                            }

                            thisState = States.ReturnToReadyMessageLine2;
                            break;

                        case States.ReturnToReadyMessageLine2:
                            if (this.deviceSerialLcd.WriteLine(DeviceSerialLcd.LineNumber.Two, String.Empty) == false)
                            {
                                if (lastError == null)
                                {
                                    lastError = this.deviceSerialLcd.LastError;
                                }
                            }

                            thisState = States.Done;
                            break;
                    }

                    /*
                     * Do not check if the experiment has been cancelled, it has finished running anyway
                     */
                }

                /*
                 * Check if there were any errors
                 */
                success = (lastError == null);
                if (success == false)
                {
                    this.executionStatus.ErrorMessage = lastError;
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                this.executionStatus.ErrorMessage = ex.Message;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

    }
}
