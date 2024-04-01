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

    /// <summary>
    /// Utility class used for logging relevant information
    /// </summary>
    public static class LoggingUtilities
    {
        /// <summary>
        /// Summary log output.
        /// </summary>
        private static readonly WeakReference<TextWriter> SummaryLog = new WeakReference<TextWriter>(null);

        /// <summary>
        /// Verbose log output.
        /// </summary>
        private static readonly WeakReference<TextWriter> VerboseLog = new WeakReference<TextWriter>(null);

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
    }
}
