using Library.LabEquipment.Engine;

namespace Library.LabEquipment
{
    public class Consts : LabConsts
    {
        /*
         * XML elements in the EquipmentConfig.xml file
         */
        public const string STRXML_Network = "network";
        public const string STRXML_IPaddr = "ipaddr";
        public const string STRXML_Port = "port";
        public const string STRXML_Timeouts = "timeouts";
        public const string STRXML_Receive = "receive";
        public const string STRXML_Modbus = "modbus";
        public const string STRXML_SlaveId = "slaveId";
        public const string STRXML_StatusCheck = "statusCheck";
        public const string STRXML_StatusCheckSpeed = "speed";
        public const string STRXML_Measurements = "measurements";
        public const string STRXML_MeasurementDelay = "measurementDelay";

        /*
         * XML elements for specification SetupIds
         */
        public const string STRXML_SetupId_NoLoad = "NoLoad";
        public const string STRXML_SetupId_FullLoad = "FullLoad";
        public const string STRXML_SetupId_LockedRotor = "LockedRotor";
        public const string STRXML_SetupId_SynchronousSpeed = "SynchronousSpeed";

        /*
         * XML elements in the specification
         */

        /*
         * XML elements in the validation
         */

        /*
         * XML elements in the results
         */
        public const string STRXML_Voltage = "voltage";
        public const string STRXML_Current = "current";
        public const string STRXML_PowerFactor = "powerFactor";
        public const string STRXML_Speed = "speed";
        public const string STRXML_ATTR_Units = "units";
        public const string STRXML_ATTR_Format = "format";
    }
}
