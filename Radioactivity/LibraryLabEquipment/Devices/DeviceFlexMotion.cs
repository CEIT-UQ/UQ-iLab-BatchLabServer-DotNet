using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using Library.Lab;
using Library.LabEquipment.Engine;
using Library.LabEquipment.Engine.Devices;
using Library.LabEquipment.Types;

namespace Library.LabEquipment.Devices
{
    public class DeviceFlexMotion : DeviceGeneric
    {
        #region Constants
        private const String STR_ClassName = "DeviceFlexMotion";
        private const Logfile.Level logLevel = Logfile.Level.Finer;
        #endregion

        #region Variables
        protected Dictionary<String, ConfigurationSource> mapSources;
        protected Dictionary<String, ConfigurationAbsorber> mapAbsorbers;
        /*
         * Tube
         */
        protected int tubeOffsetDistance;
        protected double tubeMoveRate;
        /*
         * Sources
         */
        protected char sourceFirstLocation;
        protected char sourceHomeLocation;
        /*
         * Absorbers
         */
        protected char absorberFirstLocation;
        protected char absorberHomeLocation;
        #endregion

        #region Properties
        public new static String ClassName
        {
            get { return STR_ClassName; }
        }

        protected bool absorbersPresent;
        protected int tubeDistanceHome;
        private String sourceNameHome;
        private String absorberNameHome;

        public bool AbsorbersPresent
        {
            get { return absorbersPresent; }
        }

        public int TubeDistanceHome
        {
            get { return tubeDistanceHome; }
        }

        public String SourceNameHome
        {
            get { return sourceNameHome; }
        }

        public String AbsorberNameHome
        {
            get { return absorberNameHome; }
        }
        #endregion

        //---------------------------------------------------------------------------------------//

        public DeviceFlexMotion(LabEquipmentConfiguration labEquipmentConfiguration)
            : base(labEquipmentConfiguration, STR_ClassName)
        {
            const String methodName = "DeviceFlexMotion";
            Logfile.WriteCalled(Logfile.Level.Info, STR_ClassName, methodName);

            try
            {
                /*
                 * Tube settings
                 */
                XmlNode xmlNode = XmlUtilities.GetChildNode(this.xmlNodeDevice, Consts.STRXML_Tube);
                this.tubeOffsetDistance = XmlUtilities.GetChildValueAsInt(xmlNode, Consts.STRXML_OffsetDistance);
                this.tubeDistanceHome = XmlUtilities.GetChildValueAsInt(xmlNode, Consts.STRXML_HomeDistance);
                this.tubeMoveRate = XmlUtilities.GetChildValueAsDouble(xmlNode, Consts.STRXML_MoveRate);

                /*
                 * Source settings
                 */
                xmlNode = XmlUtilities.GetChildNode(this.xmlNodeDevice, Consts.STRXML_Sources);
                this.sourceFirstLocation = XmlUtilities.GetChildValueAsChar(xmlNode, Consts.STRXML_FirstLocation);
                this.sourceHomeLocation = XmlUtilities.GetChildValueAsChar(xmlNode, Consts.STRXML_HomeLocation);

                /*
                 * Source name to configuration mapping
                 */
                this.mapSources = new Dictionary<String, ConfigurationSource>();
                ArrayList xmlNodeList = XmlUtilities.GetChildNodeList(xmlNode, Consts.STRXML_Source, false);
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    /*
                     * Add the mapping
                     */
                    XmlNode xmlNodeSource = (XmlNode)xmlNodeList[i];
                    ConfigurationSource configurationSource = ConfigurationSource.XmlParse(xmlNodeSource.OuterXml);
                    this.mapSources.Add(configurationSource.Name, configurationSource);

                    /*
                     * Check if this is the home location
                     */
                    if (configurationSource.Location == this.sourceHomeLocation)
                    {
                        this.sourceNameHome = configurationSource.Name;
                    }

                }

                /*
                 * Absorbers may not be present
                 */
                try
                {
                    xmlNode = XmlUtilities.GetChildNode(this.xmlNodeDevice, Consts.STRXML_Absorbers, true);

                    /*
                     * Absorber settings
                     */
                    this.absorberFirstLocation = XmlUtilities.GetChildValueAsChar(xmlNode, Consts.STRXML_FirstLocation);
                    this.absorberHomeLocation = XmlUtilities.GetChildValueAsChar(xmlNode, Consts.STRXML_HomeLocation);

                    /*
                     * Absorber name to configuration mapping
                     */
                    this.mapAbsorbers = new Dictionary<String, ConfigurationAbsorber>();
                    xmlNodeList = XmlUtilities.GetChildNodeList(xmlNode, Consts.STRXML_Absorber, false);
                    for (int i = 0; i < xmlNodeList.Count; i++)
                    {
                        /*
                         * Add the mapping
                         */
                        XmlNode xmlNodeAbsorber = (XmlNode)xmlNodeList[i];
                        ConfigurationAbsorber configurationAbsorber = ConfigurationAbsorber.XmlParse(xmlNodeAbsorber.OuterXml);
                        this.mapAbsorbers.Add(configurationAbsorber.Name, configurationAbsorber);

                        /*
                         * Check if this is the home location
                         */
                        if (configurationAbsorber.Location == this.absorberHomeLocation)
                        {
                            this.absorberNameHome = configurationAbsorber.Name;
                        }
                    }

                    /*
                     * Absorbers are present
                     */
                    this.absorbersPresent = true;
                }
                catch (Exception)
                {
                    /*
                     * Absorbers are not present
                     */
                    this.absorbersPresent = false;
                }
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
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Nothing to do here
                 */

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

        //---------------------------------------------------------------------------------------//

        public virtual bool EnablePower()
        {
            return true;
        }

        //---------------------------------------------------------------------------------------//

        public virtual bool DisablePower()
        {
            return true;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// Get the time in seconds it takes to move the tube from one position to another.
        /// </summary>
        /// <param name="startDistance">The distance in millimeters for the start of the move.</param>
        /// <param name="endDistance">The distance in millimeters for the end of the move.</param>
        /// <returns>double</returns>
        public virtual double GetTubeMoveTime(int startDistance, int endDistance)
        {
            return 0.0;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public double GetSourceSelectTime(String to)
        {
            double time = 0.0;

            ConfigurationSource configurationSource;
            if (this.mapSources.TryGetValue(to, out configurationSource) == true)
            {
                time = this.GetSourceSelectTime(configurationSource.Location);
            }

            return time;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public double GetSourceReturnTime(String from)
        {
            double time = 0.0;

            ConfigurationSource configurationSource;
            if (this.mapSources.TryGetValue(from, out configurationSource) == true)
            {
                time = this.GetSourceReturnTime(configurationSource.Location);
            }

            return time;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public double GetAbsorberSelectTime(String to)
        {
            double time = 0.0;

            if (this.absorbersPresent == true)
            {
                ConfigurationAbsorber configurationAbsorber;
                if (this.mapAbsorbers.TryGetValue(to, out configurationAbsorber) == true)
                {
                    time = this.GetAbsorberSelectTime(configurationAbsorber.Location);
                }
            }

            return time;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public double GetAbsorberReturnTime(String from)
        {
            double time = 0.0;

            if (this.absorbersPresent == true)
            {
                ConfigurationAbsorber configurationAbsorber;
                if (this.mapAbsorbers.TryGetValue(from, out configurationAbsorber) == true)
                {
                    time = this.GetAbsorberReturnTime(configurationAbsorber.Location);
                }
            }

            return time;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public double GetAbsorption(String name)
        {
            double absorption = 0;

            if (this.absorbersPresent == true)
            {
                ConfigurationAbsorber configurationAbsorber;
                if (this.mapAbsorbers.TryGetValue(name, out configurationAbsorber) == true)
                {
                    absorption = configurationAbsorber.Absorption;
                }
            }

            return absorption;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distance"></param>
        /// <returns>bool</returns>
        public bool SelectTubeDistance(int distance)
        {
            return this.SetTubeDistance(distance);
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns>bool</returns>
        public bool SelectSource(String name)
        {
            bool success = false;

            ConfigurationSource configurationSource;
            if (this.mapSources != null && this.mapSources.TryGetValue(name, out configurationSource) == true)
            {
                success = this.SetSourceLocation(configurationSource.Location);
            }

            return success;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns>bool</returns>
        public bool SelectAbsorber(String name)
        {
            bool success = true;

            if (this.absorbersPresent == true)
            {
                success = false;

                ConfigurationAbsorber configurationAbsorber;
                if (this.mapAbsorbers != null && this.mapAbsorbers.TryGetValue(name, out configurationAbsorber) == true)
                {
                    success = this.SetAbsorberLocation(configurationAbsorber.Location);
                }
            }

            return success;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toLocation"></param>
        /// <returns>double</returns>
        protected virtual double GetSourceSelectTime(char toLocation)
        {
            return 0.0;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromLocation"></param>
        /// <returns>double</returns>
        protected virtual double GetSourceReturnTime(char fromLocation)
        {
            return 0.0;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toLocation"></param>
        /// <returns>double</returns>
        protected virtual double GetAbsorberSelectTime(char toLocation)
        {
            return 0.0;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromLocation"></param>
        /// <returns>double</returns>
        protected virtual double GetAbsorberReturnTime(char fromLocation)
        {
            return 0.0;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distance"></param>
        /// <returns>bool</returns>
        public virtual bool SetTubeDistance(int distance)
        {
            return true;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <returns>bool</returns>
        protected virtual bool SetSourceLocation(char location)
        {
            return true;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <returns>bool</returns>
        protected virtual bool SetAbsorberLocation(char location)
        {
            return true;
        }

    }
}
