using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace UbiDisplays.Model
{
    /// <summary>
    /// A class which manages an instance of a debug log.
    /// </summary>
    /// <remarks>This is a threadsafe log.</remarks>
    public abstract class Log
    {
        /// <summary>
        /// The types of error which can be written to the log.
        /// </summary>
        public enum Type
        {
            AppInfo = 0,
            DisplayInfo = 1,

            AppWarning = 10,
            DisplayWarning = 11,

            AppError = 20,
            DisplayError = 21,
        }

        /// <summary>
        /// A log message.
        /// </summary>
        public class LogMessage
        {
            /// <summary>
            /// The string which contains details of the log message or error.
            /// </summary>
            public String Message { get; set; }
            /// <summary>
            /// The source of the log message or error (i.e. display or surface name).
            /// </summary>
            public String Source { get; set; }
            /// <summary>
            /// The type of message which changes how it is handled.
            /// </summary>
            public Type LogType { get; set; }
            /// <summary>
            /// The time which the log message refers to.
            /// </summary>
            public DateTime Time { get; set; }
            /// <summary>
            /// Any user-data such as a traceback.
            /// </summary>
            public object UserData { get; set; }

            /// <summary>
            /// Create a new log message. This will be blank but contain the current time.
            /// </summary>
            public LogMessage()
            {
                this.Time = DateTime.Now;
            }

            /// <summary>
            /// Create a new log message.
            /// </summary>
            /// <param name="sMessage">The string which contains details of the log message or error.</param>
            /// <param name="sSource">The source of the log message or error (i.e. display or surface name).</param>
            /// <param name="eType">The type of message which changes how it is handled.</param>
            public LogMessage(String sMessage, String sSource, Type eType)
            {
                this.Message = sMessage;
                this.Source = sSource;
                this.LogType = eType;
                this.Time = DateTime.Now;
            }
        }

        /// <summary>
        /// Manages thread saftey on the log.
        /// </summary>
        private static Mutex mLogMutex = new Mutex();

        /// <summary>
        /// An event which is thrown each time we recieve a new log message.
        /// </summary>
        public static event Action<LogMessage> OnNewLogMessage;

        /// <summary>
        /// A list of log messages.
        /// </summary>
        private static List<LogMessage> lMessages = new List<LogMessage>();

        /// <summary>
        /// Write data to the log.
        /// </summary>
        /// <param name="sMessage">The string which contains details of the log message or error.</param>
        /// <param name="sSource">The source of the log message or error (i.e. display or surface name).</param>
        /// <param name="eType">The type of message which changes how it is handled.</param>
        public static void Write(String sMessage, String sSource, Type eType)
        {
            // Put it into a log message.
            LogMessage pMessage = new LogMessage(sMessage, sSource, eType);

            // Acquire the mutex.
            mLogMutex.WaitOne();

            // Store the data.
            lMessages.Add(pMessage);

            // Write to the log if necessary.

            // Release the mutex.
            mLogMutex.ReleaseMutex();

            // Raise the log changed event.
            if (OnNewLogMessage != null)
                OnNewLogMessage.Invoke(pMessage);
        }
    }
}
