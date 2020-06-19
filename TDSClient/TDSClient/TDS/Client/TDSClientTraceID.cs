//  ---------------------------------------------------------------------------
//  <copyright file="TDSClientTraceID.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Client
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using TDSClient.TDS.Interfaces;
    using TDSClient.TDS.Utilities;

    /// <summary>
    /// Client Trace ID used in TDS PreLogin package.
    /// </summary>
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class TDSClientTraceID : IPackageable, IEquatable<TDSClientTraceID>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TDSClientTraceID"/> class.
        /// </summary>
        public TDSClientTraceID()
        { 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TDSClientTraceID"/> class.
        /// </summary>
        /// <param name="traceID">Trace ID</param>
        /// <param name="activityID">Activity ID</param>
        /// <param name="activitySequence">Activity Sequence</param>
        public TDSClientTraceID(byte[] traceID, byte[] activityID, uint activitySequence)
        {
            this.TraceID = traceID;
            this.ActivityID = activityID;
            this.ActivitySequence = activitySequence;
        }

        /// <summary>
        /// Gets or sets the TDS Client Application Trace ID
        /// </summary>
        public byte[] TraceID { get; set; }

        /// <summary>
        /// Gets or sets the TDS Client Application Activity ID
        /// </summary>
        public byte[] ActivityID { get; set; }

        /// <summary>
        /// Gets or sets the TDS Client Application Activity Sequence
        /// </summary>
        public uint ActivitySequence { get; set; }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as TDSClientTraceID);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public bool Equals(TDSClientTraceID other)
        {
            return other != null &&
                   (this.TraceID != null && this.TraceID.SequenceEqual(other.TraceID)) &&
                   (this.ActivityID != null && this.ActivityID.SequenceEqual(other.ActivityID)) &&
                   this.ActivitySequence == other.ActivitySequence;
        }

        /// <summary>
        /// Used to pack IPackageable to a stream.
        /// </summary>
        /// <param name="stream">MemoryStream in which IPackageable is packet into.</param>
        public void Pack(MemoryStream stream)
        {
            stream.Write(this.TraceID, 0, this.TraceID.Length);
            stream.Write(this.ActivityID, 0, this.ActivityID.Length);
            LittleEndianUtilities.WriteUInt(stream, this.ActivitySequence);
        }

        /// <summary>
        /// Used to unpack IPackageable from a stream.
        /// </summary>
        /// <param name="stream">MemoryStream from which to unpack IPackageable.</param>
        /// <returns>Returns true if successful.</returns>
        public bool Unpack(MemoryStream stream)
        {
            stream.Read(this.TraceID, 0, 16);
            stream.Read(this.ActivityID, 0, 16);
            this.ActivitySequence = LittleEndianUtilities.ReadUInt(stream);
            
            return true;
        }
    }
}