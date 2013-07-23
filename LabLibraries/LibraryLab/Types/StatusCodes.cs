using System;

namespace Library.Lab.Types
{
    public enum StatusCodes
    {
        Ready = 0,        // Ready to execute
        Waiting = 1,      // Waiting in the execution queue
        Running = 2,      // Currently running
        Completed = 3,    // Completely normally
        Failed = 4,       // Terminated with errors
        Cancelled = 5,    // Cancelled by user before execution had begun
        Unknown = 6       // Unknown experimentID
    }
}
