using System;
using System.Collections.Generic;
using System.Diagnostics;
using Library.Lab;
using Library.Lab.Utilities;
using Library.LabEquipment.Engine;
using Library.LabEquipment.Types;

namespace Library.LabEquipment.Devices
{
    public class DeviceFlexMotionSimulation : DeviceFlexMotion
    {
        #region Constants
        private const String STR_ClassName = "DeviceFlexMotionSimulation";
        private const Logfile.Level logLevel = Logfile.Level.Finer;
        /*
         * String constants for logfile messages
         */
        protected const String STRLOG_Location_arg = "Location: {0:s}";
        protected const String STRLOG_Distance_arg = "Distance: {0:d}";
        #endregion

        #region Variables
        protected Dictionary<Char, ConfigurationSource> mapSourceLocations;
        protected Dictionary<Char, ConfigurationAbsorber> mapAbsorberLocations;
        protected int currentTubeDistance;
        protected char currentSourceLocation;
        protected char currentAbsorberLocation;
        #endregion

        #region Properties
        public new static String ClassName
        {
            get { return STR_ClassName; }
        }

        private bool delaysSimulated;

        public bool DelaysSimulated
        {
            get { return delaysSimulated; }
            set { delaysSimulated = value; }
        }
        #endregion

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="labEquipmentConfiguration"></param>
        public DeviceFlexMotionSimulation(LabEquipmentConfiguration labEquipmentConfiguration)
            : base(labEquipmentConfiguration)
        {
            const String methodName = "DeviceFlexMotionSimulation";
            Logfile.WriteCalled(Logfile.Level.Info, STR_ClassName, methodName);

            try
            {
                /*
                 * Source location to configuration mapping
                 */
                this.mapSourceLocations = new Dictionary<Char, ConfigurationSource>();
                foreach (KeyValuePair<String, ConfigurationSource> item in this.mapSources)
                {
                    this.mapSourceLocations.Add(item.Value.Location, item.Value);
                }

                if (this.absorbersPresent == true)
                {
                    /*
                     * Absorber location to configuration mapping
                     */
                    this.mapAbsorberLocations = new Dictionary<Char, ConfigurationAbsorber>();
                    foreach (KeyValuePair<String, ConfigurationAbsorber> item in this.mapAbsorbers)
                    {
                        this.mapAbsorberLocations.Add(item.Value.Location, item.Value);
                    }
                }

                /*
                 * Initialise properties
                 */
                this.delaysSimulated = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw ex;
            }

            Logfile.WriteCompleted(Logfile.Level.Info, STR_ClassName, methodName);
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        public override bool Initialise()
        {
            const String methodName = "Initialise";
            Logfile.WriteCalled(STR_ClassName, methodName);

            bool success = false;

            try
            {
                this.currentTubeDistance = this.tubeDistanceHome;
                this.currentSourceLocation = this.sourceHomeLocation;
                this.currentAbsorberLocation = this.absorberHomeLocation;

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// Get the time in seconds it takes to move the tube from one position to another.
        /// </summary>
        /// <param name="startDistance">The distance in millimeters for the start of the move.</param>
        /// <param name="endDistance">The distance in millimeters for the end of the move.</param>
        /// <returns>double</returns>
        public override double GetTubeMoveTime(int startDistance, int endDistance)
        {
            double seconds = 0.0;

            /*
             * Get the absolute distance
             */
            int distance = endDistance - startDistance;
            if (distance < 0)
            {
                distance = -distance;
            }

            /*
             * Tube move rate is in seconds per millimetre
             */
            seconds = (distance * this.tubeMoveRate);

            return seconds;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toLocation"></param>
        /// <returns>double</returns>
        protected override double GetSourceSelectTime(char toLocation)
        {
            double seconds = 0.0;

            ConfigurationSource configurationSource;
            if (this.mapSourceLocations.TryGetValue(toLocation, out configurationSource) == true)
            {
                seconds = configurationSource.SelectTime;
            }

            return seconds;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromLocation"></param>
        /// <returns>double</returns>
        protected override double GetSourceReturnTime(char fromLocation)
        {
            double seconds = 0.0;

            ConfigurationSource configurationSource;
            if (this.mapSourceLocations.TryGetValue(fromLocation, out configurationSource) == true)
            {
                seconds = configurationSource.ReturnTime;
            }

            return seconds;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toLocation"></param>
        /// <returns>double</returns>
        protected override double GetAbsorberSelectTime(char toLocation)
        {
            double seconds = 0.0;

            if (this.absorbersPresent == true)
            {
                ConfigurationAbsorber configurationAbsorber;
                if (this.mapAbsorberLocations.TryGetValue(toLocation, out configurationAbsorber) == true)
                {
                    seconds = configurationAbsorber.SelectTime;
                }
            }

            return seconds;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromLocation"></param>
        /// <returns>double</returns>
        protected override double GetAbsorberReturnTime(char fromLocation)
        {
            double seconds = 0.0;

            if (this.absorbersPresent == true)
            {
                ConfigurationAbsorber configurationAbsorber;
                if (this.mapAbsorberLocations.TryGetValue(fromLocation, out configurationAbsorber) == true)
                {
                    seconds = configurationAbsorber.ReturnTime;
                }
            }

            return seconds;
        }

        //---------------------------------------------------------------------------------------//

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

            /*
             * Check if simulating delays
             */
            if (this.delaysSimulated == true)
            {
                double milliseconds = 1000.0 * this.GetTubeMoveTime(this.currentTubeDistance, distance);

                while (milliseconds > 1000.0)
                {
                    milliseconds -= 1000.0;

                    Delay.MilliSeconds(1000);
                    Trace.Write("T");
                }

                Delay.MilliSeconds((int)(milliseconds));
                Trace.WriteLine("");
            }

            this.currentTubeDistance = distance;

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return true;
        }

        //---------------------------------------------------------------------------------------//

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

            /*
             * Check if simulating delays
             */
            if (this.delaysSimulated == true)
            {
                /*
                 * Get time to move from one source to another - calculated as follows:
                 * B -> C = ReturnTime(B) + (SelectTime(C) - SelectTime(Home))
                 */
                double milliseconds = 1000.0 * (
                    this.GetSourceReturnTime(this.currentSourceLocation) +
                    this.GetSourceSelectTime(location) -
                    this.GetSourceSelectTime(this.sourceHomeLocation)
                    );

                while (milliseconds > 1000.0)
                {
                    milliseconds -= 1000.0;

                    Delay.MilliSeconds(1000);
                    Trace.Write("S");
                }

                Delay.MilliSeconds((int)(milliseconds));
                Trace.WriteLine("");
            }

            this.currentSourceLocation = location;

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return true;
        }

        //---------------------------------------------------------------------------------------//

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

            if (this.absorbersPresent == true)
            {
                /*
                 * Check if simulating delays
                 */
                if (this.delaysSimulated == true)
                {
                    /*
                     * Get time to move from one absorber to another - calculated as follows:
                     * B -> C = ReturnTime(B) + (SelectTime(C) - SelectTime(Home))
                     */
                    double milliseconds = 1000.0 * (
                        this.GetAbsorberReturnTime(this.currentAbsorberLocation) +
                        this.GetAbsorberSelectTime(location) -
                        this.GetAbsorberSelectTime(this.absorberHomeLocation)
                        );

                    while (milliseconds > 1000.0)
                    {
                        milliseconds -= 1000.0;

                        Delay.MilliSeconds(1000);
                        Trace.Write("A");
                    }

                    Delay.MilliSeconds((int)(milliseconds));
                    Trace.WriteLine("");
                }

                this.currentAbsorberLocation = location;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return true;
        }

    }
}
