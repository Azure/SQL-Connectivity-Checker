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

        public static void WriteLog(string message)
        {
            if (Log.TryGetTarget(out TextWriter temp) && temp != null)
            {
                temp.WriteLine($"[{DateTime.Now.ToString("s", DateTimeFormatInfo.InvariantInfo)}] {message}");
            }

            if (VerboseLog.TryGetTarget(out temp) && temp != null)
            {
                temp.WriteLine($"[{DateTime.Now.ToString("s", DateTimeFormatInfo.InvariantInfo)}] {message}");
            }
        }

        public static void WriteLogVerboseOnly(string message)
        {
            if (VerboseLog.TryGetTarget(out TextWriter temp) && temp != null)
            {
                temp.WriteLine($"[{DateTime.Now.ToString("s", DateTimeFormatInfo.InvariantInfo)}] {message}");
            }
        }

        public static void ClearLog()
        {
            Log.SetTarget(null);
        }

        public static void ClearVerboseLog()
        {
            VerboseLog.SetTarget(null);
        }
    }
}
