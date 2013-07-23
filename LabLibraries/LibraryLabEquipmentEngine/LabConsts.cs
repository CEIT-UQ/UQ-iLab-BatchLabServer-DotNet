
namespace Library.LabEquipment.Engine
{
    public class LabConsts
    {
        /*
         * Application configuration file key strings
         */
        public const string STRCFG_LogFilesPath = "LogFilesPath";
        public const string STRCFG_LogLevel = "LogLevel";
        public const string STRCFG_XmlEquipmentConfigFilename = "XmlEquipmentConfigFilename";
        public const string STRCFG_Authenticating = "Authenticating";
        public const string STRCFG_LogAuthentication = "LogAuthentication";
        public const string STRCFG_LabServer = "LabServer";
        /*
         * Constants for LabServer CSV string information
         */
        public const int LABSERVER_SIZE = 3;
        public const int INDEX_LABSERVER_NAME = 0;
        public const int INDEX_LABSERVER_GUID = 1;
        public const int INDEX_LABSERVER_PASSKEY = 2;
        public const char CHRCSV_SplitterChar = ',';
        /*
         * XML elements in the equipment configuration file
         */
        public const string STRXML_EquipmentConfig = "equipmentConfig";
        public const string STRXML_ATTR_Title = "title";
        public const string STRXML_ATTR_Version = "version";
        public const string STRXML_PowerupDelay = "powerupDelay";
        public const string STRXML_PowerdownTimeout = "powerdownTimeout";
        public const string STRXML_Devices = "devices";
        public const string STRXML_Device = "device";
        public const string STRXML_ATTR_Name = "name";
        public const string STRXML_InitialiseEnabled = "enabled";
        public const string STRXML_InitialiseDelay = "delay";
        public const string STRXML_Drivers = "drivers";
        public const string STRXML_Driver = "driver";
        public const string STRXML_ExecutionTimes = "executionTimes";
        public const string STRXML_Initialise = "initialise";
        public const string STRXML_Start = "start";
        public const string STRXML_Run = "run";
        public const string STRXML_Stop = "stop";
        public const string STRXML_Finalise = "finalise";
        public const string STRXML_Validation = "validation";
        public const string STRXML_Setups = "setups";
        public const string STRXML_Setup = "setup";
        public const string STRXML_ATTR_Id = "id";
        /*
         * XML elements for specification SetupIds
         */
        public const string STRXML_SetupId_Generic = "Generic";
        /*
         * XML elements in the specification
         */
        public const string STRXML_ExperimentSpecification = "experimentSpecification";
        public const string STRXML_SetupId = "setupId";
        /*
         * XML elements in the results
         */
        public const string STRXML_ExperimentResults = "experimentResults";
    }
}
