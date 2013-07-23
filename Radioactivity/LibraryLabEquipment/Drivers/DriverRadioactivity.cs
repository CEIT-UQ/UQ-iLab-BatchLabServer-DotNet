using System;
using Library.Lab;
using Library.Lab.Types;
using Library.LabEquipment.Devices;
using Library.LabEquipment.Engine;
using Library.LabEquipment.Types;

namespace Library.LabEquipment.Drivers
{
    public class DriverRadioactivity : DriverEquipment
    {
        #region Constants
        private const String STR_ClassName = "DriverRadioactivity";
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
            SelectAbsorberMessageLine1, SelectAbsorberMessageLine2, SelectAbsorber,
            SelectSourceMessageLine1, SelectSourceMessageLine2, SelectSource,
            SetTubeDistanceMessageLine1, SetTubeDistanceMessageLine2, SetTubeDistance,
            CaptureDataMessageLine1, CaptureDataMessageLine2, CaptureData,
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
        public DriverRadioactivity(LabEquipmentConfiguration labEquipmentConfiguration)
            : base(labEquipmentConfiguration)
        {
            const String methodName = "DriverRadioactivity";
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
                 * Check the setup Id
                 */
                String setupId = this.experimentSpecification.SetupId;
                switch (setupId)
                {
                    case Consts.STRXML_SetupId_RadioactivityVsTime:
                    case Consts.STRXML_SetupId_RadioactivityVsDistance:
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
                        validation = new Validation(true, this.GetExecutionTime(this.experimentSpecification));
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
             * Get absorber select time
             */
            executionTime = this.deviceSerialLcd.GetWriteLineTime() * 2;
            executionTime += this.deviceFlexMotion.GetAbsorberSelectTime(experimentSpecification.AbsorberNames[0]);

            /*
             * Get source select time
             */
            executionTime += this.deviceSerialLcd.GetWriteLineTime() * 2;
            executionTime += this.deviceFlexMotion.GetSourceSelectTime(experimentSpecification.SourceName);

            /*
             * Set starting time
             */
            this.executionTimes.Start = (int)(executionTime + 0.5);

            /*
             * Calculate running time
             */
            int[] distances = experimentSpecification.Distances;
            executionTime = 0.0;
            for (int i = 0; i < distances.Length; i++)
            {
                /*
                 * Get tube move time
                 */
                executionTime += this.deviceSerialLcd.GetWriteLineTime() * 2;
                if (i == 0)
                {
                    executionTime += this.deviceFlexMotion.GetTubeMoveTime(this.deviceFlexMotion.TubeDistanceHome, distances[0]);
                }
                else
                {
                    executionTime += this.deviceFlexMotion.GetTubeMoveTime(distances[i - 1], distances[i]);
                }

                /*
                 * Get capture data time
                 */
                executionTime += this.deviceSerialLcd.GetWriteLineTime() * 2;
                executionTime += this.deviceST360Counter.GetCaptureDataTime(experimentSpecification.Duration) * experimentSpecification.Repeat;
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
            executionTime += this.deviceFlexMotion.GetAbsorberReturnTime(experimentSpecification.AbsorberNames[0]);

            /*
             * Get tube move time from last distance to home
             */
            executionTime += this.deviceSerialLcd.GetWriteLineTime() * 2;
            executionTime += this.deviceFlexMotion.GetTubeMoveTime(distances[distances.Length - 1], this.deviceFlexMotion.TubeDistanceHome);

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

            String absorberName = this.experimentSpecification.AbsorberNames[0];
            String sourceName = this.experimentSpecification.SourceName;

            try
            {
                /*
                 * Initialise state machine
                 */
                States thisState = (this.deviceFlexMotion.AbsorbersPresent == true) ? States.SelectAbsorberMessageLine1 : States.SelectSourceMessageLine1;

                /*
                 * State machine loop
                 */
                while (thisState != States.Done)
                {
                    switch (thisState)
                    {
                        case States.SelectAbsorberMessageLine1:
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

                            thisState = States.SelectSourceMessageLine1;
                            break;

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

            String absorberName = this.experimentSpecification.AbsorberNames[0];

            try
            {
                /*
                 * Initialise local variables
                 */
                this.experimentResults.DataVectors = new int[this.experimentSpecification.Distances.Length][];
                int distanceIndex = 0;
                int repeatIndex = 0;
                int distance = 0;
                int duration = 0;
                String lcdMessage;
                States thisState = States.SetTubeDistanceMessageLine1;

                /*
                 * State machine loop
                 */
                while (thisState != States.Done)
                {
                    switch (thisState)
                    {
                        case States.SetTubeDistanceMessageLine1:
                            distance = this.experimentSpecification.Distances[distanceIndex];
                            duration = this.experimentSpecification.Duration;

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

                            /*
                             * Allocate storage for the capture data
                             */
                            this.experimentResults.DataVectors[distanceIndex] = new int[this.experimentSpecification.Repeat];
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
                            this.experimentResults.DataVectors[distanceIndex][repeatIndex] = counts;

                            /*
                             * Check if all repeats at this distance have been processed
                             */
                            if (++repeatIndex == this.experimentSpecification.Repeat)
                            {
                                /*
                                 * Check if all distances have been processed
                                 */
                                if (++distanceIndex == this.experimentSpecification.Distances.Length)
                                {
                                    thisState = States.Done;
                                    break;
                                }

                                thisState = States.SetTubeDistanceMessageLine1;
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
                String lastError = null;
                States thisState = States.ReturnSourceMessageLine1;

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

                            thisState = (this.deviceFlexMotion.AbsorbersPresent == true) ? States.ReturnAbsorberMessageLine1 : States.ReturnTubeMessageLine1;
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
