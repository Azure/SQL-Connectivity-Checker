//  ---------------------------------------------------------------------------
//  <copyright file="TDSClientTraceID.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Client
{
    using System.IO;
    using TDSClient.TDS.Interfaces;
    using TDSClient.TDS.Utilities;

    /// <summary>
    /// Client Trace ID used in TDS PreLogin package.
    /// </summary>
    public class TDSClientTraceID : IPackageable
    {
        /// <summary>
        /// TDS Client Application Trace ID
        /// </summary>
        private byte[] traceID;

        /// <summary>
        /// TDS Client Application Activity ID
        /// </summary>
        private byte[] activityID;

        /// <summary>
        /// TDS Client Application Activity Sequence
        /// </summary>
        private ulong activitySequence;

        public void Pack(MemoryStream stream)
        {
            BigEndianUtilities.WriteByteArray(stream, this.traceID);
            BigEndianUtilities.WriteByteArray(stream, this.activityID);
            BigEndianUtilities.WriteULong(stream, this.activitySequence);
        }

        public bool Unpack(MemoryStream stream)
        {
            this.traceID = BigEndianUtilities.ReadByteArray(stream, 16);
            this.activityID = BigEndianUtilities.ReadByteArray(stream, 16);
            this.activitySequence = BigEndianUtilities.ReadULong(stream);
            
            return true;
        }
    }
}