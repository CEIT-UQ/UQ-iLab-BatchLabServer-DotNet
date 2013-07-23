using System;
using System.Threading;
using Library.Lab;

namespace Library.LabEquipment.Engine
{
    public class WaitNotify
    {
        #region Constants
        public const string STR_ClassName = "WaitNotify";
        private const Logfile.Level logLevel = Logfile.Level.Finest;
        #endregion

        #region Variables
        private Object signalLock;
        private bool signal;
        #endregion

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        public WaitNotify()
        {
            const string methodName = "WaitNotify";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Initialise local variables
             */
            this.signalLock = new Object();
            this.signal = false;

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// Wait the specified number of milliseconds with 1000 millisecond resolution.
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns>bool</returns>
        public bool Wait(int milliseconds)
        {
            int seconds = (milliseconds + 500) / 1000;

            try
            {
                do
                {
                    lock (this.signalLock)
                    {
                        Monitor.Wait(this.signalLock, 1000);
                    }
                } while (this.signal == false && --seconds > 0);
            }
            catch (Exception)
            {
            }

            return this.signal;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        public void Notify()
        {
            lock (this.signalLock)
            {
                this.signal = true;
                Monitor.PulseAll(this.signalLock);
            }
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            this.signal = false;
        }
    }
}
