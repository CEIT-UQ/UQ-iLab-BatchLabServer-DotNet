using System;
using System.Runtime.InteropServices;
using System.Xml;
using Library.Lab;
using Library.LabEquipment.Engine;
using Library.LabEquipment.Types;

namespace Library.LabEquipment.Devices
{
    public class DeviceFlexMotionHardware : DeviceFlexMotionSimulation
    {
        #region Constants
        private const String STR_ClassName = "DeviceFlexMotionHardware";
        private const Logfile.Level logLevel = Logfile.Level.Finer;

        private const String STRLOG_InitialisingController = " Initialising controller...";
        private const String STRLOG_ControllerInitialised = " Controller initialised.";
        private const String STRLOG_PowerupResetInitialisation = "Powerup initialisation...";
        private const String STRLOG_FindingTubeForwardLimit = " Finding tube forward limit...";
        private const String STRLOG_FindingTubeReverseLimit = " Finding tube reverse limit...";
        private const String STRLOG_ResettingTubePosition = " Resetting tube position...";
        private const String STRLOG_SettingTubeDistanceToHomePosition = " Setting tube distance to home position...";
        private const String STRLOG_SettingSourceToHomeLocation = " Setting source to home location...";
        private const String STRLOG_SettingAbsorberToHomeLocation = " Setting absorber to home location...";
        private const String STRLOG_Done = " Done.";

        private const String STRERR_FlexMotionBoardNotPresent = "FlexMotion board is not present!";
        private const String STRERR_PowerEnableBreakpointFailed = "Failed to set PowerEnable breakpoint!";
        private const String STRERR_CounterStartBreakpointFailed = "Failed to set CounterStart breakpoint!";
        private const String STRERR_FindTubeReverseLimitFailed = "FindTubeReverseLimit Failed!";
        private const String STRERR_ResetTubePositionFailed = "ResetTubePosition Failed!";
        private const String STRERR_FindTubeForwardLimitFailed = "FindTubeForwardLimit Failed!";
        private const String STRERR_SetTubeDistanceFailed = "SetTubeDistance Failed!";
        private const String STRERR_SetSourceLocationFailed = "SetSourceLocation Failed!";
        private const String STRERR_SetAbsorberLocationFailed = "SetAbsorberLocation Failed!";
        private const String STRERR_InvalidLocation = " Invalid Location: ";

        // Tube axis encoder counts per mm distance moved
        private const int ENCODER_COUNTS_PER_MM = 43000;

        #endregion

        #region Variables
        private byte boardId;
        private byte tubeAxis;
        private byte sourceAxis;
        private byte absorberAxis;
        private byte unusedAxis;
        private byte powerEnableBreakpoint;
        private byte counterStartBreakpoint;
        #endregion

        #region Properties

        public new static String ClassName
        {
            get { return STR_ClassName; }
        }

        private bool powerupReset;

        public bool PowerupReset
        {
            get { return powerupReset; }
            set { powerupReset = value; }
        }

        #endregion

        #region DLL Import

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_initialize_controller(byte boardId, byte[] settings);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_read_csr_rtn(byte boardId, ref ushort csr);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_find_reference(byte boardId, byte axisOrVectorSpace, ushort axisOrVSMap, byte searchType);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_check_reference(byte boardId, byte axisOrVectorSpace, ushort axisOrVSMap, ref ushort found, ref ushort finding);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_reset_pos(byte boardId, byte axis, int position1, int position2, byte inputVector);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_read_pos_rtn(byte boardId, byte axis, ref int position);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_set_op_mode(byte boardId, byte axis, ushort operationMode);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_load_target_pos(byte boardId, byte axis, int targetPosition, byte inputVector);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_start(byte boardId, byte axisOrVectorSpace, ushort axisOrVSMap);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_stop_motion(byte boardId, byte axisOrVectorSpace, ushort stopType, ushort axisOrVSMap);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_read_axis_status_rtn(byte boardId, byte axis, ref ushort axisStatus);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_read_error_msg_rtn(byte boardId, ref ushort commandID, ref ushort resourceID, ref int errorCode);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_get_error_description(ushort descriptionType, int errorCode, ushort commandID, ushort resourceID, char[] charArray, ref int sizeOfArray);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_set_breakpoint_output_momo(byte boardId, byte axisOrEncoder, ushort mustOn, ushort mustOff, byte inputVector);

        [DllImport("FlexMotion32.dll")]
        private static extern int flex_enable_breakpoint(byte boardId, byte axisOrEncoder, byte enable);

        #endregion

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="labEquipmentConfiguration"></param>
        public DeviceFlexMotionHardware(LabEquipmentConfiguration labEquipmentConfiguration)
            : base(labEquipmentConfiguration)
        {
            const String methodName = "DeviceFlexMotionHardware";
            Logfile.WriteCalled(Logfile.Level.Info, STR_ClassName, methodName);

            try
            {
                /*
                 * Hardware settings
                 */
                XmlNode xmlNodeHardware = XmlUtilities.GetChildNode(this.xmlNodeDevice, Consts.STRXML_Hardware);
                this.boardId = (byte)XmlUtilities.GetChildValueAsInt(xmlNodeHardware, Consts.STRXML_BoardId);

                /*
                 * Tube axis
                 */
                XmlNode xmlNodeAxisId = XmlUtilities.GetChildNode(xmlNodeHardware, Consts.STRXML_AxisId);
                int axisId = XmlUtilities.GetChildValueAsInt(xmlNodeAxisId, Consts.STRXML_Tube);
                this.tubeAxis = this.GetNimcAxis(axisId);

                /*
                 * Source axis
                 */
                axisId = XmlUtilities.GetChildValueAsInt(xmlNodeAxisId, Consts.STRXML_Sources);
                this.sourceAxis = this.GetNimcAxis(axisId);

                /*
                 * Absorber axis may not be specified
                 */
                try
                {
                    axisId = XmlUtilities.GetChildValueAsInt(xmlNodeAxisId, Consts.STRXML_Absorbers);
                    this.absorberAxis = this.GetNimcAxis(axisId);
                    axisId = XmlUtilities.GetChildValueAsInt(xmlNodeAxisId, Consts.STRXML_Unused);
                    this.unusedAxis = this.GetNimcAxis(axisId);
                }
                catch
                {
                    this.absorberAxis = Nimc.NOAXIS;
                    this.unusedAxis = Nimc.NOAXIS;
                }

                /*
                 * Set breakpoint axes
                 */
                this.powerEnableBreakpoint = this.tubeAxis;
                this.counterStartBreakpoint = this.sourceAxis;

                /*
                 * Initialise the Flexmotion controller card. Must be done here because a breakpoint on the controller card
                 * is used to powerup the equipment and initialisation is carried out after the equipment is powered up.
                 */
                Logfile.Write(STRLOG_InitialisingController);

                if (this.InitialiseController() == false)
                {
                    throw new ArgumentException(this.lastError);
                }
                Logfile.Write(STRLOG_ControllerInitialised);

                /*
                 * Determine the initialise delay
                 */
                int powerupResetDelay = XmlUtilities.GetChildValueAsInt(xmlNodeHardware, Consts.STRXML_PowerupResetDelay);
                if (this.powerupReset == true)
                {
                    this.initialiseDelay += powerupResetDelay;
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw ex;
            }

            Logfile.WriteCompleted(Logfile.Level.Info, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool Initialise()
        {
            const String methodName = "Initialise";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Check if powerup reset is required
                 */
                if (this.powerupReset == true)
                {
                    Logfile.Write(STRLOG_PowerupResetInitialisation);

                    /*
                     * Find the reverse limit switch
                     */
                    Logfile.Write(STRLOG_FindingTubeReverseLimit);
                    int reversePosition = 0;
                    if (this.FindTubeReverseLimit(ref reversePosition) == false)
                    {
                        throw new ArgumentException(STRERR_FindTubeReverseLimitFailed + this.lastError);
                    }
                    Logfile.Write(STRLOG_Done);

                    /*
                     * Set tube position to zero
                     */
                    Logfile.Write(STRLOG_ResettingTubePosition);
                    if (this.ResetTubePosition() == false)
                    {
                        throw new ArgumentException(STRERR_ResetTubePositionFailed + this.lastError);
                    }
                    Logfile.Write(STRLOG_Done);

                    /*
                     * Set tube to its home position
                     */
                    Logfile.Write(STRLOG_SettingTubeDistanceToHomePosition);
                    if (this.SetTubeDistance(this.tubeDistanceHome) == false)
                    {
                        throw new ArgumentException(STRERR_SetTubeDistanceFailed + this.lastError);
                    }

                    /*
                     * Set source to its home location
                     */
                    Logfile.Write(STRLOG_SettingSourceToHomeLocation);
                    if (this.SetSourceLocation(this.sourceHomeLocation) == false)
                    {
                        throw new ArgumentException(STRERR_SetSourceLocationFailed + this.lastError);
                    }

                    /*
                     * Set absorber to its home location
                     */
                    if (this.absorbersPresent == true)
                    {
                        Logfile.Write(STRLOG_SettingAbsorberToHomeLocation);
                        if (this.SetAbsorberLocation(this.absorberHomeLocation) == false)
                        {
                            throw new ArgumentException(STRERR_SetAbsorberLocationFailed + this.lastError);
                        }
                    }
                }

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool EnablePower()
        {
            const String methodName = "EnablePower";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Ensure power-enable and counter-start signals are inactive
                 */
                if (this.SetBreakpoint(this.powerEnableBreakpoint, false) == false)
                {
                    throw new ArgumentException(STRERR_PowerEnableBreakpointFailed + this.lastError);
                }
                if (this.SetBreakpoint(this.counterStartBreakpoint, false) == false)
                {
                    throw new ArgumentException(STRERR_CounterStartBreakpointFailed + this.lastError);
                }

                /*
                 * Toggle the counter-start signal to enable both signals
                 */
                if (this.SetBreakpoint(this.counterStartBreakpoint, true) == false)
                {
                    throw new ArgumentException(STRERR_CounterStartBreakpointFailed + this.lastError);
                }
                if (this.SetBreakpoint(this.counterStartBreakpoint, false) == false)
                {
                    throw new ArgumentException(STRERR_CounterStartBreakpointFailed + this.lastError);
                }

                /*
                 * Enable the power
                 */
                if (this.SetBreakpoint(this.powerEnableBreakpoint, true) == false)
                {
                    throw new ArgumentException(STRERR_PowerEnableBreakpointFailed + this.lastError);
                }

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool DisablePower()
        {
            const String methodName = "DisablePower";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 *  Make the counter-start and power-enable signals inactive
                 */
                if (this.SetBreakpoint(this.counterStartBreakpoint, false) == false)
                {
                    throw new ArgumentException(STRERR_CounterStartBreakpointFailed + this.lastError);
                }
                if (this.SetBreakpoint(this.powerEnableBreakpoint, false) == false)
                {
                    throw new ArgumentException(STRERR_PowerEnableBreakpointFailed + this.lastError);
                }

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distance"></param>
        /// <returns>bool</returns>
        public override bool SetTubeDistance(int distance)
        {
            const String methodName = "SetTubeDistance";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Distance_arg, distance));

            bool success = true;

            /*
             * Convert target position in millimetres to encoder counts
             */
            int targetPosition = (distance - this.tubeOffsetDistance) * ENCODER_COUNTS_PER_MM;

            /*
             * Move tube to target position
             */
            success = SetTubePosition(targetPosition);

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <returns>bool</returns>
        protected override bool SetSourceLocation(char location)
        {
            const String methodName = "SetSourceLocation";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Location_arg, location));

            bool success = true;

            ConfigurationSource configurationSource;
            if (this.mapSourceLocations.TryGetValue(location, out configurationSource) == true)
            {
                success = this.SetSourceEncoderPosition(configurationSource.EncoderPosition);
            }

            this.currentSourceLocation = location;

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <returns>bool</returns>
        protected override bool SetAbsorberLocation(char location)
        {
            const String methodName = "SetAbsorberLocation";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Location_arg, location));

            bool success = true;

            if (this.absorbersPresent == true)
            {
                ConfigurationAbsorber configurationAbsorber;
                if (this.mapAbsorberLocations.TryGetValue(location, out configurationAbsorber) == true)
                {
                    success = this.SetAbsorberEncoderPosition(configurationAbsorber.EncoderPosition);
                }

                this.currentAbsorberLocation = location;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return success;
        }

        //=================================================================================================//

        private byte GetNimcAxis(int axisId)
        {
            switch (axisId)
            {
                case 1:
                    return Nimc.AXIS1;

                case 2:
                    return Nimc.AXIS2;

                case 3:
                    return Nimc.AXIS3;

                case 4:
                    return Nimc.AXIS4;

                default:
                    return Nimc.NOAXIS;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        private bool InitialiseController()
        {
            bool success = false;
            int err = 0;
            ushort csr = 0;
            int state = 0;
            this.powerupReset = false;

            try
            {
                this.ClearErrors();

                while (success == false)
                {
                    switch (state)
                    {
                        case 0:
                            /*
                             * Get communication status register
                             */
                            err = flex_read_csr_rtn(this.boardId, ref csr);
                            break;

                        case 1:
                            /*
                             * Check if controller board is in powerup reset condition
                             */
                            if ((csr & Nimc.POWER_UP_RESET) != 0)
                            {
                                /*
                                 * Initialise the controller board
                                 */
                                err = flex_initialize_controller(this.boardId, null);

                                /*
                                 * Axes must be initialised after powerup reset
                                 */
                                this.powerupReset = true;
                            }
                            break;

                        case 2:
                            /*
                             * Inhibit the tube axis motor
                             */
                            err = flex_stop_motion(this.boardId, this.tubeAxis, Nimc.KILL_STOP, 0);
                            break;

                        case 3:
                            /*
                             * Inhibit the source table motor
                             */
                            err = flex_stop_motion(this.boardId, this.sourceAxis, Nimc.KILL_STOP, 0);
                            break;

                        case 4:
                            if (this.absorberAxis != Nimc.NOAXIS)
                            {
                                /*
                                 * Inhibit the absorber table motor
                                 */
                                err = flex_stop_motion(this.boardId, this.absorberAxis, Nimc.KILL_STOP, 0);
                            }
                            break;

                        case 5:
                            if (this.unusedAxis != Nimc.NOAXIS)
                            {
                                /*
                                 * Inhibit the unused axis
                                 */
                                err = flex_stop_motion(this.boardId, this.unusedAxis, Nimc.KILL_STOP, 0);
                            }
                            break;

                        case 6:
                            /*
                             * Initialisation successful
                             */
                            success = true;
                            break;
                    }

                    /*
                     * Check for errors
                     */
                    if (err != 0)
                    {
                        /*
                         * Initialisation failed
                         */
                        ProcessError(boardId, err);

                        /*
                         * Inhibit all motors
                         */
                        flex_stop_motion(this.boardId, this.tubeAxis, Nimc.KILL_STOP, 0);
                        flex_stop_motion(this.boardId, this.sourceAxis, Nimc.KILL_STOP, 0);
                        flex_stop_motion(this.boardId, this.absorberAxis, Nimc.KILL_STOP, 0);
                        flex_stop_motion(this.boardId, this.unusedAxis, Nimc.KILL_STOP, 0);
                        break;
                    }

                    /*
                     * Next state
                     */
                    state++;
                }
            }
            catch
            {
            }

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        private bool SetSourceEncoderPosition(int targetPosition)
        {
            int err = 0;
            ushort csr = 0;
            ushort found = 0;
            ushort finding = 0;
            int position = 0;
            ushort axisStatus = 0;
            int state = 0;

            while (true)
            {
                switch (state)
                {
                    case 0:
                        err = flex_find_reference(boardId, Nimc.AXIS_CTRL, (ushort)(1 << sourceAxis),
                            Nimc.FIND_HOME_REFERENCE);
                        break;

                    case 1:
                        err = flex_check_reference(boardId, Nimc.AXIS_CTRL, (ushort)(1 << sourceAxis),
                            ref found, ref finding);
                        break;

                    case 2:
                        if (finding != 0)
                        {
                            state = 1;
                            continue;
                        }
                        break;

                    case 3:
                        err = flex_read_pos_rtn(boardId, sourceAxis, ref position);
                        break;

                    case 4:
                        err = flex_reset_pos(boardId, sourceAxis, 0, 0, 0xFF);
                        break;

                    case 5:
                        err = flex_read_pos_rtn(boardId, sourceAxis, ref position);
                        break;

                    case 6:
                        err = flex_load_target_pos(boardId, sourceAxis, targetPosition, 0xFF);
                        break;

                    case 7:
                        err = flex_start(boardId, sourceAxis, 0);
                        break;

                    case 8:
                        err = flex_read_pos_rtn(boardId, sourceAxis, ref position);
                        break;

                    case 9:
                        err = flex_read_axis_status_rtn(boardId, sourceAxis, ref axisStatus);
                        break;

                    case 10:
                        // Check the modal errors
                        err = flex_read_csr_rtn(boardId, ref csr);
                        break;

                    case 11:
                        if ((csr & Nimc.MODAL_ERROR_MSG) != 0)
                        {
                            // Stop the Motion
                            flex_stop_motion(boardId, sourceAxis, Nimc.DECEL_STOP, 0);
                            err = (short)(csr & Nimc.MODAL_ERROR_MSG);
                        }
                        break;

                    case 12:
                        // Test against the move complete bit
                        if ((axisStatus & (Nimc.MOVE_COMPLETE_BIT | Nimc.AXIS_OFF_BIT)) == 0)
                        {
                            // Not finished yet
                            state = 8;
                            continue;
                        }
                        break;

                    case 13:
                        // Inhibit the motor
                        err = flex_stop_motion(this.boardId, this.sourceAxis, Nimc.KILL_STOP, 0);
                        break;

                    case 14:
                        // Successful
                        return (true);
                }

                // Check for errors
                if (err != 0)
                {
                    // Failed
                    ProcessError(boardId, err);

                    // Inhibit the motor
                    flex_stop_motion(this.boardId, this.sourceAxis, Nimc.KILL_STOP, 0);

                    Logfile.WriteError("state: " + state.ToString() + "  err: " + err.ToString());

                    return (false);
                }

                state++;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        private bool SetAbsorberEncoderPosition(int targetPosition)
        {
            int err = 0;
            ushort csr = 0;
            ushort found = 0;
            ushort finding = 0;
            int position = 0;
            ushort axisStatus = 0;
            int state = 0;

            for (; ; )
            {
                switch (state)
                {
                    case 0:
                        err = flex_find_reference(boardId, Nimc.AXIS_CTRL, (ushort)(1 << absorberAxis),
                            Nimc.FIND_HOME_REFERENCE);
                        break;

                    case 1:
                        err = flex_check_reference(boardId, Nimc.AXIS_CTRL, (ushort)(1 << absorberAxis),
                            ref found, ref finding);
                        break;

                    case 2:
                        if (finding != 0)
                        {
                            state = 1;
                            continue;
                        }
                        break;

                    case 3:
                        err = flex_read_pos_rtn(boardId, absorberAxis, ref position);
                        break;

                    case 4:
                        err = flex_reset_pos(boardId, absorberAxis, 0, 0, 0xFF);
                        break;

                    case 5:
                        err = flex_read_pos_rtn(boardId, absorberAxis, ref position);
                        break;

                    case 6:
                        err = flex_load_target_pos(boardId, absorberAxis, targetPosition, 0xFF);
                        break;

                    case 7:
                        err = flex_start(boardId, absorberAxis, 0);
                        break;

                    case 8:
                        err = flex_read_pos_rtn(boardId, absorberAxis, ref position);
                        break;

                    case 9:
                        err = flex_read_axis_status_rtn(boardId, absorberAxis, ref axisStatus);
                        break;

                    case 10:
                        // Check the modal errors
                        err = flex_read_csr_rtn(boardId, ref csr);
                        break;

                    case 11:
                        if ((csr & Nimc.MODAL_ERROR_MSG) != 0)
                        {
                            // Stop the Motion
                            flex_stop_motion(boardId, absorberAxis, Nimc.DECEL_STOP, 0);
                            err = (short)(csr & Nimc.MODAL_ERROR_MSG);
                        }
                        break;

                    case 12:
                        // Test against the move complete bit
                        if ((axisStatus & (Nimc.MOVE_COMPLETE_BIT | Nimc.AXIS_OFF_BIT)) == 0)
                        {
                            // Not finished yet
                            state = 8;
                            continue;
                        }
                        break;

                    case 13:
                        // Inhibit the motor
                        err = flex_stop_motion(this.boardId, this.absorberAxis, Nimc.KILL_STOP, 0);
                        break;

                    case 14:
                        // Successful
                        return (true);
                }

                // Check for errors
                if (err != 0)
                {
                    // Failed
                    ProcessError(boardId, err);

                    // Inhibit the motor
                    flex_stop_motion(this.boardId, this.absorberAxis, Nimc.KILL_STOP, 0);

                    return (false);
                }

                state++;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        private bool FindTubeForwardLimit(ref int position)
        {
            int err = 0;
            ushort found = 0;
            ushort finding = 0;
            int state = 0;

            for (; ; )
            {
                switch (state)
                {
                    case 0:
                        // Find the reverse limit switch
                        err = flex_find_reference(boardId, Nimc.AXIS_CTRL, (ushort)(1 << tubeAxis),
                            Nimc.FIND_FORWARD_LIMIT_REFERENCE);
                        break;

                    case 1:
                        err = flex_check_reference(boardId, Nimc.AXIS_CTRL, (ushort)(1 << tubeAxis),
                            ref found, ref finding);
                        break;

                    case 2:
                        if (finding != 0)
                        {
                            state = 1;
                            continue;
                        }
                        break;

                    case 3:
                        // Read the current position of the tube
                        err = flex_read_pos_rtn(boardId, tubeAxis, ref position);
                        break;

                    case 4:
                        // Inhibit the motor
                        err = flex_stop_motion(boardId, tubeAxis, Nimc.KILL_STOP, 0);
                        break;

                    case 5:
                        // Successful
                        return true;
                }

                // Check for errors
                if (err != 0)
                {
                    // Failed
                    ProcessError(boardId, err);

                    // Inhibit the motor
                    flex_stop_motion(boardId, tubeAxis, Nimc.KILL_STOP, 0);

                    return false;
                }

                // Next state
                state++;

            }
        }

        //-------------------------------------------------------------------------------------------------//

        private bool FindTubeReverseLimit(ref int position)
        {
            int err = 0;
            ushort found = 0;
            ushort finding = 0;
            int state = 0;

            for (; ; )
            {
                switch (state)
                {
                    case 0:
                        // Find the reverse limit switch
                        err = flex_find_reference(boardId, Nimc.AXIS_CTRL, (ushort)(1 << tubeAxis),
                            Nimc.FIND_REVERSE_LIMIT_REFERENCE);
                        break;

                    case 1:
                        err = flex_check_reference(boardId, Nimc.AXIS_CTRL, (ushort)(1 << tubeAxis),
                            ref found, ref finding);
                        break;

                    case 2:
                        if (finding != 0)
                        {
                            state = 1;
                            continue;
                        }
                        break;

                    case 3:
                        // Read the current position of the tube
                        err = flex_read_pos_rtn(boardId, tubeAxis, ref position);
                        break;

                    case 4:
                        // Inhibit the motor
                        err = flex_stop_motion(boardId, tubeAxis, Nimc.KILL_STOP, 0);
                        break;

                    case 5:
                        // Successful
                        return true;
                }

                // Check for errors
                if (err != 0)
                {
                    // Failed
                    ProcessError(boardId, err);

                    // Inhibit the motor
                    flex_stop_motion(boardId, tubeAxis, Nimc.KILL_STOP, 0);

                    return false;
                }

                // Next state
                state++;

            }
        }

        //-------------------------------------------------------------------------------------------------//

        private bool ResetTubePosition()
        {
            int err = 0;
            int position = 0;
            int state = 0;

            for (; ; )
            {
                switch (state)
                {
                    case 0:
                        // Read the position of the reverse limit switch
                        err = flex_read_pos_rtn(boardId, tubeAxis, ref position);
                        break;

                    case 1:
                        // Reset the position to 0
                        err = flex_reset_pos(boardId, tubeAxis, 0, 0, 0xFF);
                        break;

                    case 2:
                        // Read the position again
                        err = flex_read_pos_rtn(boardId, tubeAxis, ref position);
                        break;

                    case 3:
                        // Successful
                        return true;
                }

                // Check for errors
                if (err != 0)
                {
                    // Failed
                    ProcessError(boardId, err);
                    return false;
                }

                // Next state
                state++;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        private bool GetTubePosition(ref int position)
        {
            int err = 0;
            int state = 0;

            for (; ; )
            {
                switch (state)
                {
                    case 0:
                        // Read the current position of the tube
                        err = flex_read_pos_rtn(boardId, tubeAxis, ref position);
                        break;

                    case 1:
                        // Successful
                        return true;
                }

                // Check for errors
                if (err != 0)
                {
                    // Failed
                    ProcessError(boardId, err);
                    return false;
                }

                // Next state
                state++;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        private bool SetTubePosition(int targetPosition)
        {
            int err = 0;
            ushort csr = 0;
            int position = 0;
            ushort axisStatus = 0;
            int state = 0;

            for (; ; )
            {
                switch (state)
                {
                    case 0:
                        err = flex_set_op_mode(boardId, tubeAxis, Nimc.ABSOLUTE_POSITION);
                        break;

                    case 1:
                        err = flex_read_pos_rtn(boardId, tubeAxis, ref position);
                        break;

                    case 2:
                        err = flex_load_target_pos(boardId, tubeAxis, targetPosition, 0xFF);
                        break;

                    case 3:
                        err = flex_start(boardId, tubeAxis, 0);
                        break;

                    case 4:
                        err = flex_read_pos_rtn(boardId, tubeAxis, ref position);
                        break;

                    case 5:
                        err = flex_read_axis_status_rtn(boardId, tubeAxis, ref axisStatus);
                        break;

                    case 6:
                        // Check the modal errors
                        err = flex_read_csr_rtn(boardId, ref csr);
                        break;

                    case 7:
                        if ((csr & Nimc.MODAL_ERROR_MSG) != 0)
                        {
                            // Stop the Motion
                            err = flex_stop_motion(boardId, tubeAxis, Nimc.DECEL_STOP, 0);
                        }
                        break;

                    case 8:
                        // Test against the move complete bit
                        if ((axisStatus & (Nimc.MOVE_COMPLETE_BIT | Nimc.AXIS_OFF_BIT)) == 0)
                        {
                            // Not finished yet
                            state = 4;
                            continue;
                        }

                        // Inhibit the motor
                        err = flex_stop_motion(boardId, tubeAxis, Nimc.KILL_STOP, 0);
                        break;

                    case 9:
                        // Successful
                        return true;
                }

                // Check for errors
                if (err != 0)
                {
                    // Failed
                    ProcessError(boardId, err);

                    // Inhibit the motor
                    flex_stop_motion(boardId, tubeAxis, Nimc.KILL_STOP, 0);

                    return false;
                }

                state++;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        private bool SetBreakpoint(byte axis, bool enable)
        {
            int err = 0;
            int state = 0;

            ushort muston = 0, mustoff = 0;

            if (enable)
            {
                mustoff = (ushort)(1 << axis);
            }
            else
            {
                muston = (ushort)(1 << axis);
            }

            for (; ; )
            {
                switch (state)
                {
                    case 0:
                        // Disable breakpoint to allow direct control of I/O
                        err = flex_enable_breakpoint(boardId, axis, 0);
                        break;

                    case 1:
                        err = flex_set_breakpoint_output_momo(boardId, axis, muston, mustoff, 0xFF);
                        break;

                    case 2:
                        err = flex_set_breakpoint_output_momo(boardId, axis, 0, 0, 0xFF);
                        break;

                    case 3:
                        // Successful
                        return true;
                }

                // Check for errors
                if (err != 0)
                {
                    // Failed
                    ProcessError(boardId, err);
                    return false;
                }

                // Next state
                state++;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        private void ClearErrors()
        {
            int err = 0;
            ushort csr = 0;
            ushort commandID = 0;
            ushort resourceID = 0;
            int errorCode = 0;

            try
            {
                while (true)
                {
                    err = flex_read_csr_rtn(boardId, ref csr);
                    if ((csr & Nimc.MODAL_ERROR_MSG) == 0)
                    {
                        return;
                    }

                    flex_read_error_msg_rtn(boardId, ref commandID, ref resourceID, ref errorCode);
                }
            }
            catch
            {
                throw new Exception("FlexMotion controller access failed");
            }
        }

        //-------------------------------------------------------------------------------------------------//

        private void ProcessError(byte boardId, int error)
        {
            int err = 0;
            ushort csr = 0;
            ushort commandID = 0;
            ushort resourceID = 0;
            int errorCode = 0;

            err = flex_read_csr_rtn(boardId, ref csr);
            if ((csr & Nimc.MODAL_ERROR_MSG) != 0)
            {
                do
                {
                    /*
                     * Get the command ID, resource and the error code of the modal error from the error stack on the board.
                     */
                    err = flex_read_error_msg_rtn(boardId, ref commandID, ref resourceID, ref errorCode);
                    this.lastError = GetErrorDescription(errorCode, commandID, resourceID);

                    err = flex_read_csr_rtn(boardId, ref csr);
                } while ((csr & Nimc.MODAL_ERROR_MSG) != 0);
            }
            else
            {
                this.lastError = GetErrorDescription(error, 0, 0);
            }
        }

        //-------------------------------------------------------------------------------------------------//

        private string GetErrorDescription(int errorCode, ushort commandID, ushort resourceID)
        {
            char[] errorDescription = null;
            int sizeOfArray = 0;
            ushort descriptionType;

            descriptionType = (commandID == 0) ? Nimc.ERROR_ONLY : Nimc.COMBINED_DESCRIPTION;

            /*
             * First, get the size for the error description
             */
            flex_get_error_description(descriptionType, errorCode, commandID, resourceID,
                                errorDescription, ref sizeOfArray);

            /*
             * sizeOfArray is size of description + NULL character
             */
            sizeOfArray++;

            /*
             * Allocate char array for the description
             */
            errorDescription = new char[sizeOfArray];

            /*
             * Get error description
             */
            flex_get_error_description(descriptionType, errorCode, commandID, resourceID,
                                    errorDescription, ref sizeOfArray);

            return new string(errorDescription);
        }
    }
}
