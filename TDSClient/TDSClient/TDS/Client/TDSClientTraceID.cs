//  ---------------------------------------------------------------------------
//  <copyright file="TDSClientTraceID.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Client
{
    using System;
    using System.IO;
    using System.Linq;
    using TDSClient.TDS.Interfaces;
    using TDSClient.TDS.Utilities;

    /// <summary>
    /// Client Trace ID used in TDS PreLogin package.
    /// </summary>
    #pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class TDSClientTraceID : IPackageable, IEquatable<TDSClientTraceID>
    {
    #pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        /// <summary>
        /// Gets or sets the TDS Client Application Trace ID
        /// </summary>
        public byte[] TraceID { get; }

        /// <summary>
        /// Gets or sets the TDS Client Application Activity ID
        /// </summary>
        public byte[] ActivityID { get; }

        /// <summary>
        /// Gets or sets the TDS Client Application Activity Sequence
        /// </summary>
        public uint ActivitySequence { get; set;  }

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
            TraceID = traceID ?? throw new ArgumentNullException(nameof(traceID));
            ActivityID = activityID ?? throw new ArgumentNullException(nameof(activityID));
            ActivitySequence = activitySequence;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as TDSClientTraceID);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        
        public bool Equals(TDSClientTraceID other)
        {
            return other != null &&
                TraceID.SequenceEqual(other.TraceID) &&
                ActivityID.SequenceEqual(other.ActivityID) &&
                ActivitySequence == other.ActivitySequence;
        }

        /// <summary>
        /// Used to pack IPackageable to a stream.
        /// </summary>
        /// <param name="stream">MemoryStream in which IPackageable is packet into.</param>
        public void Pack(MemoryStream stream)
        {
            stream.Write(TraceID, 0, TraceID.Length);
            stream.Write(ActivityID, 0, ActivityID.Length);
            LittleEndianUtilities.WriteUInt(stream, ActivitySequence);
        }

        /// <summary>
        /// Used to unpack IPackageable from a stream.
        /// </summary>
        /// <param name="stream">MemoryStream from which to unpack IPackageable.</param>
        /// <returns>Returns true if successful.</returns>
        public bool Unpack(MemoryStream stream)
        {
            stream.Read(TraceID, 0, 16);
            stream.Read(ActivityID, 0, 16);
            ActivitySequence = LittleEndianUtilities.ReadUInt(stream);
            
            return true;
        }
    }
}