//  ---------------------------------------------------------------------------
//  <copyright file="LoggingUtilities.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Utilities
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Utility class used for logging relevant information
    /// </summary>
    public static class LoggingUtilities
    {
        /// <summary>
        /// Object lock for log writer
        /// </summary>
        private static readonly object logWriterLock = new object();

        /// <summary>
        /// Summary log output.
        /// </summary>
        private static readonly WeakReference<TextWriter> SummaryLog = new WeakReference<TextWriter>(null);

        /// <summary>
        /// Verbose log output.
        /// </summary>
        private static readonly WeakReference<TextWriter> VerboseLog = new WeakReference<TextWriter>(null);

        /// <summary>
        /// MSAL log output.
        /// </summary>
        private static readonly WeakReference<TextWriter> MsalLog = new WeakReference<TextWriter>(null);

        /// <summary>
        /// Datetime format use in the logs.
        /// </summary>
        private static readonly string DatetimeFormat = "yyyy.MM.dd HH:mm:ss.ffff";

        /// <summary>
        /// Used to set Log output.
        /// </summary>
        /// <param name="log">Log output.</param>
        public static void SetSummaryLog(TextWriter log)
        {
            if (!SummaryLog.TryGetTarget(out TextWriter temp) || temp == null)
            {
                SummaryLog.SetTarget(log);
            }
            else
            {
                throw new InvalidOperationException("SummaryLog is already set!");
            }
        }

        /// <summary>
        /// Used to set Verbose Log output.
        /// </summary>
        /// <param name="log">Verbose log output.</param>
        public static void SetVerboseLog(TextWriter log)
        {
            if (!VerboseLog.TryGetTarget(out TextWriter temp) || temp == null)
            {
                VerboseLog.SetTarget(log);
            }
            else
            {
                throw new InvalidOperationException("VerboseLog is already set!");
            }
        }

        /// <summary>
        /// Used to write message to Log and Verbose Log. 
        /// </summary>
        /// <param name="message">Message to write to Log.</param>
        /// <param name="writeToVerboseLog">Option to write to verbose log.</param>
        /// <param name="writeToSummaryLog">Option to write to summary log.</param>
        public static void WriteLog(string message, bool writeToVerboseLog = true, bool writeToSummaryLog = false)
        {
            var timestamp = DateTime.UtcNow.ToString(DatetimeFormat, DateTimeFormatInfo.InvariantInfo);
            if (writeToSummaryLog && SummaryLog.TryGetTarget(out TextWriter temp) && temp != null)
            {
                temp.WriteLine($"[{timestamp}] {message}");
            }

            if (writeToVerboseLog && VerboseLog.TryGetTarget(out temp) && temp != null)
            {
                temp.WriteLine($"[{timestamp}] {message}");
            }
        }

        /// <summary>
        /// Used to write empty line to Log and Verbose Log. 
        /// </summary>
        /// <param name="writeToVerboseLog">Option to write to verbose log.</param>
        /// <param name="writeToSummaryLog">Option to write to summary log.</param>
        public static void AddEmptyLine(bool writeToVerboseLog = true, bool writeToSummaryLog = false)
        {
            if (writeToSummaryLog && SummaryLog.TryGetTarget(out TextWriter temp) && temp != null)
            {
                temp.WriteLine();
            }

            if (writeToVerboseLog && VerboseLog.TryGetTarget(out temp) && temp != null)
            {
                temp.WriteLine();
            }
        }

        /// <summary>
        /// Used to remove Log output.
        /// </summary>
        public static void ClearSummaryLog()
        {
            SummaryLog.SetTarget(null);
        }

        /// <summary>
        /// Used to remove Verbose Log output.
        /// </summary>
        public static void ClearVerboseLog()
        {
            VerboseLog.SetTarget(null);
        }

        /// <summary>
        /// Used to remove Msal Log output.
        /// </summary>
        public static void ClearMsalLog()
        {
            VerboseLog.SetTarget(null);
        }

        /// <summary>
        /// Log object content into destination
        /// </summary>
        /// <param name="log">Destination</param>
        /// <param name="prefix">Prefix the output with</param>
        /// <param name="instance">Object to log</param>
        public static void Log(TextWriter log, string prefix, object instance)
        {
            // Check log validity
            if (log == null)
            {
                // Don't log anything
                return;
            }

            // Check if null
            if (instance == null)
            {
                SerializedWriteLineToLog(log, string.Format("{0}: <null>", prefix));

                return;
            }

            // Get object type
            Type objectType = instance.GetType();

            // Check if simple type
            if (objectType.IsEnum
                || instance is bool
                || instance is string
                || instance is int
                || instance is uint
                || instance is byte
                || instance is sbyte
                || instance is short
                || instance is ushort
                || instance is long
                || instance is ulong
                || instance is double
                || instance is float
                || instance is Version)
            {
                SerializedWriteLineToLog(log, string.Format("{0}: {1}", prefix, instance));

                return;
            }

            // Check declaring type
            if (objectType.IsGenericType || (objectType.BaseType != null && objectType.BaseType.IsGenericType))  // IList<T>
            {
                int index = 0;

                // Log values
                foreach (object o in instance as System.Collections.IEnumerable)
                {
                    Log(log, string.Format("{0}[{1}]", prefix, index++), o);
                }

                // Check if we logged anything
                if (index == 0)
                {
                    SerializedWriteLineToLog(log, string.Format("{0}: <empty>", prefix));
                }
            }
            else if (objectType.IsArray)
            {
                // Prepare prefix
                string preparedLine = string.Format("{0}: [", prefix);

                // Log values
                foreach (object o in instance as Array)
                {
                    preparedLine += string.Format("{0:X} ", o);
                }

                // Finish the line
                preparedLine += "]";

                // Move to the next line
                SerializedWriteLineToLog(log, preparedLine);
            }

            // Iterate all public properties
            foreach (PropertyInfo info in objectType.GetProperties())
            {
                // Check if this is an indexer
                if (info.GetIndexParameters().Length > 0 || !info.DeclaringType.Assembly.Equals(Assembly.GetExecutingAssembly()))
                {
                    // We ignore indexers
                    continue;
                }

                // Get property value
                object value = info.GetValue(instance, null);

                // Log each property
                Log(log, string.Format("{0}.{1}.{2}", prefix, objectType.Name, info.Name), value);
            }

            // Flush to destination
            lock (logWriterLock)
            {
                log.Flush();
            }
        }

        /// <summary>
        /// Serialized write line to destination
        /// </summary>
        /// <param name="log">Destination</param>
        /// <param name="text">Text to log</param>        
        public static void SerializedWriteLineToLog(TextWriter log, string text)
        {
            lock (logWriterLock)
            {
                log.WriteLine(string.Format("[{0}] {1}", DateTime.Now, text));
            }
        }
    }
}
