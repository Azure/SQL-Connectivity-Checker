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

    public static class LoggingUtilities
    {
        private static readonly WeakReference<TextWriter> Log = new WeakReference<TextWriter>(null);
        private static readonly WeakReference<TextWriter> VerboseLog = new WeakReference<TextWriter>(null);

        /// <summary>
        /// Used to set Log output.
        /// </summary>
        /// <param name="log">Log output.</param>
        public static void SetLog(TextWriter log)
        {
            if (!Log.TryGetTarget(out TextWriter temp) || temp == null)
            {
                Log.SetTarget(log);
            }
            else
            {
                throw new InvalidOperationException("Log is already set!");
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
        public static void WriteLog(string message)
        {
            if (Log.TryGetTarget(out TextWriter temp) && temp != null)
            {
                temp.WriteLine($"[{DateTime.UtcNow.ToString("s", DateTimeFormatInfo.InvariantInfo)}] {message}");
            }

            if (VerboseLog.TryGetTarget(out temp) && temp != null)
            {
                temp.WriteLine($"[{DateTime.UtcNow.ToString("s", DateTimeFormatInfo.InvariantInfo)}] {message}");
            }
        }

        /// <summary>
        /// Used to write message to Verbose Log only.
        /// </summary>
        /// <param name="message">Message to write to Log.</param>
        public static void WriteLogVerboseOnly(string message)
        {
            if (VerboseLog.TryGetTarget(out TextWriter temp) && temp != null)
            {
                temp.WriteLine($"[{DateTime.UtcNow.ToString("s", DateTimeFormatInfo.InvariantInfo)}] {message}");
            }
        }

        /// <summary>
        /// Used to remove Log output.
        /// </summary>
        public static void ClearLog()
        {
            Log.SetTarget(null);
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
