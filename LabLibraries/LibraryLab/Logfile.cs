using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Library.Lab
{
    public class Logfile
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "Logfile";

        //
        // Logging levels
        //
        public enum Level
        {
            All = 0,
            Finest = 1,
            Finer = 2,
            Fine = 3,
            Info = 4,
            Config = 5,
            Warning = 6,
            Severe = 7,
            Off = 8
        }

        //
        // String constants
        //
        public const string STRLOG_LogLevel = " LogLevel:  ";
        public const string STRLOG_Newline = "\r\n";
        public const string STRLOG_Spacer = "  ";
        public const string STRLOG_Quote = "'";

        private const string DEFAULT_EXT = ".log";

        private const string STRLOG_LogFilePath = " LogFilePath: ";
        private const string STRLOG_TimestampFormat = "hh:mm:ss.ff tt";
        private const string STRLOG_TimestampSpacer = ":  ";
        private const string STRLOG_TimestampEmpty = "              ";
        private const string STRLOG_NewLine = "\r\n";
        private const string STRLOG_Called = "(): Called";
        private const string STRLOG_CalledMarker = " >> ";
        private const string STRLOG_Completed = "(): Completed";
        private const string STRLOG_CompletedMarker = " << ";
        private const string STRLOG_CreateThreadObjectsFailed = "Create thread objects failed!";
        private const string STR_Error = " ***ERROR*** ";
        private const string STRLOG_InnerExceptionType = "Inner Exception Type: ";
        private const string STRLOG_InnerException = "Inner Exception: ";
        private const string STRLOG_InnerSource = "Inner Source: ";
        private const string STRLOG_InnerStackTrace = "Inner Stack Trace: ";
        private const string STRLOG_ExceptionType = "Exception Type: ";
        private const string STRLOG_Exception = "Exception: ";
        private const string STRLOG_Source = "Source: ";
        private const string STRLOG_StackTrace = "Stack Trace: ";

        private static string logfilePath = null;
        private static Queue logfileQueue = null;
        private static Level logLevel = Level.Info;

        //
        // Thread related local variables and properties
        //
        private static bool running;
        private static Object statusLock;
        private static Thread threadLogfile;

        //
        // Private properties
        //
        private static bool Running
        {
            get
            {
                lock (statusLock)
                {
                    return running;
                }
            }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public static void SetFilePath(string path)
        {
            //
            // Save logfile physical path and create dircetory if it doesn't already exist
            //
            logfilePath = path;
            Directory.CreateDirectory(path);

            //
            // Attempt to write to logfile
            //
            string filename = CreateDatedFilename(logfilePath);
            try
            {
                StreamWriter sw = new StreamWriter(filename, true);
                sw.WriteLine(sw.NewLine + STRLOG_LogFilePath + STRLOG_Quote + path + STRLOG_Quote);
                sw.Close();
            }
            catch (Exception)
            {
                // Unable to write to logfile
                throw;
            }

            // Create queue for logfile messages
            if (logfileQueue == null)
            {
                logfileQueue = Queue.Synchronized(new Queue());
            }

            //
            // Thread related initialisation
            //
            statusLock = new Object();
            threadLogfile = new Thread(new ThreadStart(LogfileThread));
            if (statusLock == null || threadLogfile == null)
            {
                throw new ArgumentNullException(STRLOG_CreateThreadObjectsFailed);
            }

            //
            // Start the thread running
            //
            running = true;
            threadLogfile.Start();
        }

        //-------------------------------------------------------------------------------------------------//

        public static void SetLevel(Level level)
        {
            if (level <= Level.Off && level >= Level.All)
            {
                logLevel = level;
                Write(STRLOG_LogLevel + level.ToString());
            }
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Terminate the logging thread. Messages can still be written but the logfile
        /// will be opened, written and closed for each message.
        /// </summary>
        public static void Close()
        {
            //
            // Tell LogfileThread() that it is no longer running
            //
            lock (statusLock)
            {
                running = false;
            }

            if (threadLogfile != null)
            {
                //
                // Wait for LabEquipmentThread() to terminate
                //
                for (int i = 0; i < 3; i++)
                {
                    if (threadLogfile.Join(1000) == true)
                    {
                        // Thread has terminated
                        Trace.WriteLine(" LogfileThread terminated OK");
                        break;
                    }
                }
            }

            //
            // Delete thread related objects
            //
            logfileQueue = null;
            statusLock = null;
            threadLogfile = null;
        }

        //-------------------------------------------------------------------------------------------------//

        public static void Write()
        {
            Write(string.Empty);
        }

        //-------------------------------------------------------------------------------------------------//

        public static void Write(string message)
        {
            Write(Level.Info, message);
        }

        //-------------------------------------------------------------------------------------------------//

        public static void Write(Level level, string message)
        {
            if (message != null && level >= logLevel)
            {
                //
                // Add timestamp to message
                //
                DateTime now = DateTime.Now;
                message = now.ToString(STRLOG_TimestampFormat) + STRLOG_TimestampSpacer + message;

                //Trace.WriteLine(message);

                if (running == true)
                {
                    //
                    // Add the message to the queue
                    //
                    lock (logfileQueue.SyncRoot)
                    {
                        logfileQueue.Enqueue(message);
                    }
                }
                else
                {
                    try
                    {
                        //
                        // Write the message to the log file
                        //
                        string filename = CreateDatedFilename(logfilePath);
                        string fullpath = Path.GetFullPath(filename);
                        StreamWriter sw = new StreamWriter(filename, true);
                        sw.WriteLine(message);
                        sw.Close();
                    }
                    catch
                    {
                        // Message not written to log file
                    }
                }
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public static void WriteError(string message)
        {
            if (message != null)
            {
                Write(Level.Severe, STR_Error + message);
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public static void WriteError(string methodName, string message)
        {
            if (methodName != null)
            {
                message = methodName + "(): " + message;
            }
            WriteError(message);
        }

        //-------------------------------------------------------------------------------------------------//

        public static void WriteError(string className, string methodName, string message)
        {
            if (methodName != null)
            {
                message = methodName + "(): " + message;
            }
            if (className != null && methodName != null)
            {
                message = className + "." + message;
            }
            WriteError(message);
        }

        //-------------------------------------------------------------------------------------------------//

        public static void WriteException(Exception ex)
        {
            string message = string.Empty;

            if (ex.InnerException != null)
            {
                message += STRLOG_NewLine + STRLOG_InnerExceptionType + ex.InnerException.GetType().ToString();
                message += STRLOG_NewLine + STRLOG_InnerException + ex.InnerException.Message;
                message += STRLOG_NewLine + STRLOG_InnerSource + ex.InnerException.Source;
                if (ex.InnerException.StackTrace != null)
                {
                    message += STRLOG_NewLine + STRLOG_InnerStackTrace + STRLOG_NewLine + ex.InnerException.StackTrace;
                }
            }
            message += STRLOG_NewLine + STRLOG_ExceptionType + ex.GetType().ToString();
            message += STRLOG_NewLine + STRLOG_Exception + ex.Message;
            message += STRLOG_NewLine + STRLOG_Source + ex.Source;
            if (ex.StackTrace != null)
            {
                message += STRLOG_NewLine + STRLOG_StackTrace + STRLOG_NewLine + ex.StackTrace;
            }
            WriteError(message);
        }

        //-------------------------------------------------------------------------------------------------//

        public static void WriteCalled(string className, string methodName)
        {
            WriteCalled(Level.Finest, className, methodName, null);
        }

        //-------------------------------------------------------------------------------------------------//

        public static void WriteCalled(string className, string methodName, string logMessage)
        {
            WriteCalled(Level.Finest, className, methodName, logMessage);
        }

        //-------------------------------------------------------------------------------------------------//

        public static void WriteCalled(Level level, string className, string methodName)
        {
            WriteCalled(level, className, methodName, null);
        }

        //-------------------------------------------------------------------------------------------------//

        public static void WriteCalled(Level level, string className, string methodName, string logMessage)
        {
            string message = null;
            if (methodName != null)
            {
                message = methodName;
            }
            if (className != null && methodName != null)
            {
                message = className + "." + message;
            }
            if (message != null)
            {
                message = message + STRLOG_Called;
                if (logMessage != null)
                {
                    message = message + STRLOG_NewLine + STRLOG_TimestampEmpty + STRLOG_CalledMarker + logMessage;
                }
                Write(level, message);
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public static void WriteCompleted(string className, string methodName)
        {
            WriteCompleted(Level.Finest, className, methodName, null);
        }

        //-------------------------------------------------------------------------------------------------//

        public static void WriteCompleted(string className, string methodName, string logMessage)
        {
            WriteCompleted(Level.Finest, className, methodName, logMessage);
        }

        //-------------------------------------------------------------------------------------------------//

        public static void WriteCompleted(Level level, string className, string methodName)
        {
            WriteCompleted(level, className, methodName, null);
        }

        //-------------------------------------------------------------------------------------------------//

        public static void WriteCompleted(Level logLevel, string className, string methodName, string logMessage)
        {
            string message = null;
            if (methodName != null)
            {
                message = methodName;
            }
            if (className != null && methodName != null)
            {
                message = className + "." + message;
            }
            if (message != null)
            {
                message = message + STRLOG_Completed;
                if (logMessage != null)
                {
                    message = STRLOG_CompletedMarker + logMessage + STRLOG_NewLine + STRLOG_TimestampEmpty + message;
                }
                Write(logLevel, message);
            }
        }

        //=================================================================================================//

        private static void LogfileThread()
        {
            const string STRLOG_MethodName = "LogfileThread";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            while (Running == true)
            {
                // Wait a bit before checking the experiment queue
                Thread.Sleep(1000);

                //
                // Check for queued messages
                //
                lock (logfileQueue.SyncRoot)
                {
                    if (logfileQueue.Count == 0)
                    {
                        continue;
                    }

                    //Trace.WriteLine(STRLOG_ClassName + ": logfileQueue.Count => " + logfileQueue.Count.ToString());

                    //
                    // Write queued messages to logfile
                    //
                    try
                    {
                        // Open logfile for writing
                        StreamWriter sw = new StreamWriter(CreateDatedFilename(logfilePath), true);

                        // Write queued messages to the log file
                        while (logfileQueue.Count > 0)
                        {
                            // Get the next message off the queue
                            string message = (string)logfileQueue.Dequeue();

                            // Write the message to the logfile
                            sw.WriteLine(message);
                        }

                        // Close the log file
                        sw.Close();
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                    }

                    //Trace.WriteLine("Logfile closed.");
                }
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        private static string CreateDatedFilename(string path)
        {
            //
            // The current date becomes the name of the file
            //
            DateTime now = DateTime.Now;
            string filename = now.ToString("yyyy") + now.ToString("MM") + now.ToString("dd") + DEFAULT_EXT;

            if (path != null)
            {
                filename = Path.Combine(path, filename);
            }

            return filename;
        }
    }
}
