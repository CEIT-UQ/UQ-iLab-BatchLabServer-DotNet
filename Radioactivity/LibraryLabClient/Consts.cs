using System;
using Library.LabClient.Engine;

namespace Library.LabClient
{
    public class Consts : LabConsts
    {
        /*
         * String constants for XML setup Ids
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
         * XML elements in the LabConfiguration
         */
        public const String STRXML_Sources = "sources";
        public const String STRXML_Absorbers = "absorbers";
        public const String STRXML_ATTR_Default = "default";
        public const String STRXML_Source = "source";
        public const String STRXML_Absorber = "absorber";
        //public const String STRXML_Name = "name";
        public const String STRXML_Distances = "distances";
        public const String STRXML_Minimum = "minimum";
        public const String STRXML_Maximum = "maximum";
        public const String STRXML_Stepsize = "stepsize";
        /*
         * XML elements in the validation
         */
        public const String STRXML_ValidationDistance = "vdnDistance";
        public const String STRXML_ValidationDuration = "vdnDuration";
        public const String STRXML_ValidationRepeat = "vdnRepeat";
        public const String STRXML_ValidationTotaltime = "vdnTotaltime";
        /*
         * XML elements in the experiment specification
         */
        public const String STRXML_SourceName = "sourceName";
        public const String STRXML_AbsorberName = "absorberName";
        public const String STRXML_Distance = "distance";
        public const String STRXML_Duration = "duration";
        public const String STRXML_Repeat = "repeat";
        /*
         * XML elements in the experiment result
         */
        public const String STRXML_DataVector = "dataVector";
    }
}
