using System;
using System.Diagnostics;
using System.Xml;
using Library.Lab;
using Library.Lab.Utilities;
using Library.LabEquipment.Engine;

namespace Library.LabEquipment.Devices
{
    public class DeviceST360CounterSimulation : DeviceST360Counter
    {
        #region Constants
        private const String STR_ClassName = "DeviceST360CounterSimulation";
        private const Logfile.Level logLevel = Logfile.Level.Finer;
        /*
         * Delays are in millisecs
         */
        protected const int DELAY_MS_Display = 1000;
        #endregion

        #region Variables
        private double simDistance;
        private int simDuration;
        private int simMean;
        private double simPower;
        private double simDeviation;
        private Random random;
        #endregion

        #region Properties

        public new static String ClassName
        {
            get { return STR_ClassName; }
        }

        private bool delaysSimulated;
        private double absorption;
        private int distance;

        public bool DelaysSimulated
        {
            get { return delaysSimulated; }
            set { delaysSimulated = value; }
        }

        public double Absorption
        {
            get { return absorption; }
            set { absorption = value; }
        }

        public int Distance
        {
            get { return distance; }
            set { distance = value; }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="labEquipmentConfiguration"></param>
        public DeviceST360CounterSimulation(LabEquipmentConfiguration labEquipmentConfiguration)
            : base(labEquipmentConfiguration)
        {
            const String methodName = "DeviceST360CounterSimulation";
            Logfile.WriteCalled(Logfile.Level.Info, STR_ClassName, methodName);

            try
            {
                /*
                 * Simulation settings
                 */
                XmlNode node = XmlUtilities.GetChildNode(this.xmlNodeDevice, Consts.STRXML_Simulation);
                this.simDistance = XmlUtilities.GetChildValueAsDouble(node, Consts.STRXML_SimDistance);
                this.simDuration = XmlUtilities.GetChildValueAsInt(node, Consts.STRXML_SimDuration);
                this.simMean = XmlUtilities.GetChildValueAsInt(node, Consts.STRXML_SimMean);
                this.simPower = XmlUtilities.GetChildValueAsDouble(node, Consts.STRXML_SimPower);
                this.simDeviation = XmlUtilities.GetChildValueAsDouble(node, Consts.STRXML_SimDeviation);

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

        //-------------------------------------------------------------------------------------------------//

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
                /*
                 * Create the random number generator and randomise the seed
                 */
                int seed = (int)DateTime.Now.Millisecond;
                this.random = new Random(seed);

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

        //-------------------------------------------------------------------------------------------------//

        public override double GetCaptureDataTime(int duration)
        {
            double seconds = duration;

            seconds += (double)(DELAY_MS_Display) / 1000.0;

            /*
             * y = Mx + C
             */
            seconds = (seconds * this.timeAdjustmentCapture[0]) + this.timeAdjustmentCapture[1];

            return seconds;
        }

        //-------------------------------------------------------------------------------------------------//

        public override bool CaptureData(int duration, out int counts)
        {
            const String methodName = "CaptureData";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_DistanceDuration_arg2, distance, duration));

            /*
             * Check if simulating delays
             */
            if (this.delaysSimulated == true)
            {
                for (int i = 0; i < duration; i++)
                {
                    Delay.MilliSeconds(1000);
                    Trace.Write("D");
                }
                Trace.WriteLine("");
            }

            counts = this.GenerateData(this.absorption, this.distance, duration);

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Counts_arg, counts));

            return true;
        }

        //=================================================================================================//

        /// <summary>
        /// Generate simulated radioactivity data for the specified distance and duration
        /// </summary>
        /// <param name="distance">The distance in millimeters from the source</param>
        /// <param name="duration">The time in seconds to take the measurement</param>
        /// <returns></returns>
        private int GenerateData(double absorption, int distance, int duration)
        {
            /*
             * Generate a value from a Gaussian distribution of random numbers
             */
            double dataGaussian = GetGaussian(random);

            /*
             * Adjust data for duration and distance
             */
            dataGaussian = AdjustData(dataGaussian, duration, distance);

            /*
             * Adjust data for absorption where 0 percent = no absorption
             */
            if (absorption > 0)
            {
                dataGaussian = (dataGaussian * (100.0 - absorption)) / 100.0;
            }

            /*
             * Convert the simulated data from 'double' to 'int'
             */
            int value = (int)(dataGaussian + 0.5);

            /*
             * The value cannot be negative
             */
            if (value < 0)
            {
                value = 0;
            }

            return value;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Adjust the data for the mean, standard deviation, duration and distance.
        /// </summary>
        /// <param name="data">The array of data to adjust</param>
        /// <param name="duration">The time in seconds to adjust the data</param>
        /// <param name="distance">The distance to adjust the data</param>
        /// <returns></returns>
        private double AdjustData(double data, double duration, double distance)
        {
            /*
             * Calculate the scaling factors
             */
            double adjustStdDev = this.simDeviation * distance / this.simDistance;
            double adjustDuration = duration / this.simDuration;
            double adjustDistance = Math.Pow(distance / this.simDistance, this.simPower);

            /*
             * Adjust for the mean and standard deviation
             */
            double value = data * adjustStdDev + this.simMean;

            /*
             * Now adjust for the duration
             */
            value *= adjustDuration;

            /*
             * Finally adjust for the distance
             */
            value /= adjustDistance;

            return value;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Generate a Gaussian distribution of data with a mean of 0.0 and a standard deviation of 1.0
        /// using the Box–Muller transform method.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        private double GetGaussian(Random random)
        {
            double random1;
            while (true)
            {
                /*
                 * random1 must be > 0.0 for Math.log()
                 */
                random1 = random.NextDouble();
                if (random1 > 0.0)
                {
                    break;
                }
            }
            double random2 = random.NextDouble();

            double gaussian1 = Math.Sqrt(-2.0 * Math.Log(random1)) * Math.Cos(Math.PI * 2.0 * random2);

            /*
             * Don't need the second number
             * double gaussian2 = Math.sqrt(-2.0 * Math.log(random1)) * Math.sin(Math.PI * 2.0 * random2);
             */

            return gaussian1;
        }
    }
}
