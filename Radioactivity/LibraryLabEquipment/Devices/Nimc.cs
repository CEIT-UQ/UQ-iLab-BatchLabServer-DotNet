using System;

namespace Library.LabEquipment.Devices
{
    public class Nimc
    {
        public const byte BOARD_ID = 1;

        // These are the base CTRL values for resourses
        public const byte AXIS_CTRL         = 0x00;
        public const byte VECTOR_SPACE_CTRL = 0x10;
        public const byte ENCODER_CTRL      = 0x20;
        public const byte DAC_CTRL          = 0x30;
        public const byte STEP_OUTPUT_CTRL  = 0x40;
        public const byte ADC_CTRL          = 0x50;
        public const byte MIX_AXVS_CTRL     = 0x60;
        public const byte IO_PORT_CTRL      = 0x00;
        public const byte PWM_CTRL          = 0x00;
        public const byte PROGRAM_CTRL      = 0x00;

        public const byte NOAXIS = 0x00;
        public const byte AXIS1 = 0x01;
        public const byte AXIS2 = 0x02;
        public const byte AXIS3 = 0x03;
        public const byte AXIS4 = 0x04;
        public const byte AXIS5 = 0x05;
        public const byte AXIS6 = 0x06;
        public const byte AXIS7 = 0x07;
        public const byte AXIS8 = 0x08;

        // Communication Status Register bits
        public const ushort READY_TO_RECEIVE        = 0x01;		//Ready to receive
        public const ushort DATA_IN_RDB             = 0x02;		//Data in return data buffer
        public const ushort PACKET_ERROR            = 0x10;		//Packet error
        public const ushort POWER_UP_RESET          = 0x20;		//Power up Reset
        public const ushort MODAL_ERROR_MSG         = 0x40;		//Modal error message
        public const ushort HARDWARE_FAIL           = 0x80;		//Hardware Fail bit

        // Read Axis Status bits
        public const ushort RUN_STOP_BIT            = 0x0001;   //Axis running bit
        public const ushort PROFILE_COMPLETE_BIT    = 0x0002;   //Profile complete bit in 'per Axis' hardware status
        public const ushort AXIS_OFF_BIT            = 0x0004;   //Motor off bit
        public const ushort FOLLOWING_ERROR_BIT     = 0x0008;   //Following error bit
        public const ushort LIMIT_SWITCH_BIT        = 0x0010;   //Hardware Limit
        public const ushort HOME_SWITCH_BIT         = 0x0020;   //Home switch bit
        public const ushort SW_LIMIT_BIT            = 0x0040;   //Software Limit
        public const ushort VELOCITY_THRESHOLD_BIT  = 0x0100;   //Velocity threshold
        public const ushort POS_BREAKPOINT_BIT      = 0x0200;   //Position Breakpoint
        public const ushort HOME_FOUND_BIT          = 0x0400;   //Home Found
        public const ushort INDEX_FOUND_BIT         = 0x0800;   //Index Found
        public const ushort HIGH_SPEED_CAPTURE_BIT  = 0x1000;   //High Speed capture
        public const ushort DIRECTION_BIT           = 0x2000;   //Direction
        public const ushort BLEND_STATUS_BIT        = 0x4000;   //Blend Status
        public const ushort MOVE_COMPLETE_BIT       = 0x8000;   //Move Complete 

        //Find Reference
        public const byte FIND_HOME_REFERENCE           = 0x00;
        public const byte FIND_INDEX_REFERENCE          = 0x01;
        public const byte FIND_CENTER_REFERENCE         = 0x02;
        public const byte FIND_FORWARD_LIMIT_REFERENCE  = 0x03;
        public const byte FIND_REVERSE_LIMIT_REFERENCE  = 0x04;
        public const byte FIND_SEQUENCE_REFERENCE       = 0x05;
        public const byte MAX_FIND_TYPES                = 5;

        //Read Reference Status
        public const byte HOME_FOUND                = 0x0;
        public const byte INDEX_FOUND               = 0x1;
        public const byte CENTER_FOUND              = 0x2;
        public const byte FORWARD_LIMIT_FOUND       = 0x3;
        public const byte REVERSE_LIMIT_FOUND       = 0x4;
        public const byte REFERENCE_FOUND           = 0x5;
        public const byte CURRENT_SEQUENCE_PHASE    = 0x6;
        public const byte FINDING_REFERENCE         = 0x7;

        // Stop Control Modes
        public const byte DECEL_STOP    = 0;
        public const byte HALT_STOP     = 1;
        public const byte KILL_STOP     = 2;

        // Operation modes
        public const ushort ABSOLUTE_POSITION = 0;

        // These are used for getting error descriptions
        public const byte ERROR_ONLY            = 0;
        public const byte FUNCTION_NAME_ONLY    = 1;
        public const byte RESOURCE_NAME_ONLY    = 2;
        public const byte COMBINED_DESCRIPTION  = 3;

    }
}
