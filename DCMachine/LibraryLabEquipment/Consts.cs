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
        public const string STRXML_SetupId_VoltageVsSpeed = "VoltageVsSpeed";
        public const string STRXML_SetupId_VoltageVsField = "VoltageVsField";
        public const string STRXML_SetupId_VoltageVsLoad = "VoltageVsLoad";
        public const string STRXML_SetupId_SpeedVsVoltage = "SpeedVsVoltage";
        public const string STRXML_SetupId_SpeedVsField = "SpeedVsField";

        /*
         * XML elements in the specification
         */
        public const string STRXML_SpeedMin = "speedMin";
        public const string STRXML_SpeedMax = "speedMax";
        public const string STRXML_SpeedStep = "speedStep";
        public const string STRXML_FieldMin = "fieldMin";
        public const string STRXML_FieldMax = "fieldMax";
        public const string STRXML_FieldStep = "fieldStep";
        public const string STRXML_LoadMin = "loadMin";
        public const string STRXML_LoadMax = "loadMax";
        public const string STRXML_LoadStep = "loadStep";

        /*
         * XML elements in the validation
         */
        public const string STRXML_Speed = "speed";
        public const string STRXML_Field = "field";
        public const string STRXML_Load = "load";
        public const string STRXML_Minimum = "minimum";
        public const string STRXML_Maximum = "maximum";
        public const string STRXML_StepMin = "stepMin";
        public const string STRXML_StepMax = "stepMax";

        /*
         * XML elements in the results
         */
        public const string STRXML_SpeedVector = "speedVector";
        public const string STRXML_FieldVector = "fieldVector";
        public const string STRXML_VoltageVector = "voltageVector";
        public const string STRXML_LoadVector = "loadVector";
        public new const string STRXML_ATTR_Name = "name";
        public const string STRXML_ATTR_Units = "units";
        public const string STRXML_ATTR_Format = "format";
    }
}
