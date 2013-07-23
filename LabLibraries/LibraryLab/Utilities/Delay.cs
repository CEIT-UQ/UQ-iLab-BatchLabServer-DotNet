using System;
using System.Threading;

namespace Library.Lab.Utilities
{
    public class Delay
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">The number of milliseconds to delay execution.</param>
        public static void MilliSeconds(int value)
        {
            try
            {
                Thread.Sleep(value);
            }
            catch (Exception)
            {
            }
        }
    }
}
