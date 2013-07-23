using System;
using Library.LabEquipment.Engine;

namespace Library.LabEquipment
{
    public class Consts : LabConsts
    {
        /*
         * XML elements in the EquipmentConfig.xml file
         */
        public const String STRXML_Type = "type";
        public const String STRXML_Simulation = "simulation";
        public const String STRXML_Hardware = "hardware";
        public const String STRXML_BoardId = "boardId";
        public const String STRXML_AxisId = "axisId";
        public const String STRXML_Tube = "tube";
        public const String STRXML_Sources = "sources";
        public const String STRXML_Absorbers = "absorbers";
        public const String STRXML_Unused = "unused";
        public const String STRXML_PowerupResetDelay = "powerupResetDelay";
        public const String STRXML_OffsetDistance = "offsetDistance";
        public const String STRXML_HomeDistance = "homeDistance";
        public const String STRXML_MoveRate = "moveRate";
        public const String STRXML_Source = "source";
        public const String STRXML_Absorber = "absorber";
        public const String STRXML_FirstLocation = "firstLocation";
        public const String STRXML_HomeLocation = "homeLocation";
        public const String STRXML_SimDistance = "distance";
        public const String STRXML_SimDuration = "duration";
        public const String STRXML_SimMean = "mean";
        public const String STRXML_SimPower = "power";
        public const String STRXML_SimDeviation = "deviation";
        public const String STRXML_Network = "network";
        public const String STRXML_IPaddr = "ipaddr";
        public const String STRXML_Port = "port";
        public const String STRXML_Serial = "serial";
        public const String STRXML_Baud = "baud";
        public const String STRXML_WriteLineTime = "writeLineTime";
        public const String STRXML_RadiationCounter = "radiationCounter";
        public const String STRXML_GeigerTubeVoltage = "geigerTubeVoltage";
        public const String STRXML_SpeakerVolume = "speakerVolume";
        public const String STRXML_TimeAdjustment = "timeAdjustment";
        public const String STRXML_Capture = "capture";
        public const String STRXML_TypeNone = "None";
        public const String STRXML_TypeSimulation = "Simulation";
        public const String STRXML_TypeHardware = "Hardware";
        public const String STRXML_TypeSerial = "Serial";
        public const String STRXML_TypeNetwork = "Network";
        /*
         * XML elements for specification SetupIds
         */
        public const String STRXML_SetupId_RadioactivityVsTime = "RadioactivityVsTime";
        public const String STRXML_SetupId_RadioactivityVsDistance = "RadioactivityVsDistance";
        public const String STRXML_SetupId_RadioactivityVsAbsorber = "RadioactivityVsAbsorber";
        public const String STRXML_SetupId_SimActivityVsTime = "SimActivityVsTime";
        public const String STRXML_SetupId_SimActivityVsDistance = "SimActivityVsDistance";
        public const String STRXML_SetupId_SimActivityVsAbsorber = "SimActivityVsAbsorber";
        public const String STRXML_SetupId_SimActivityVsTimeNoDelay = "SimActivityVsTimeNoDelay";
        public const String STRXML_SetupId_SimActivityVsDistanceNoDelay = "SimActivityVsDistanceNoDelay";
        public const String STRXML_SetupId_SimActivityVsAbsorberNoDelay = "SimActivityVsAbsorberNoDelay";
        /*
         * XML elements in the specification
         */
        public const String STRXML_SourceName = "sourceName";
        public const String STRXML_AbsorberName = "absorberName";
        public const String STRXML_Distance = "distance";
        public const String STRXML_Duration = "duration";
        public const String STRXML_Repeat = "repeat";
        /*
         * XML elements in the validation
         */
        public const String STRXML_VdnDistance = "distance";
        public const String STRXML_VdnDuration = "duration";
        public const String STRXML_VdnRepeat = "repeat";
        public const String STRXML_VdnTotaltime = "totaltime";
        public const String STRXML_VdnMinimum = "minimum";
        public const String STRXML_VdnMaximum = "maximum";
        /*
         * XML elements in the results
         */
        public const String STRXML_DataType = "dataType";
        public const String STRXML_DataVector = "dataVector";
        public const String STRXML_ATTR_AbsorberName = "absorberName";
        public const String STRXML_ATTR_Distance = "distance";
        public const String STRXML_ATTR_Units = "units";
    }
}
