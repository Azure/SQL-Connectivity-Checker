//  ---------------------------------------------------------------------------
//  <copyright file="TDSClientVersion.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Client
{
    using System;
    using System.IO;
    using TDSClient.TDS.Interfaces;
    using TDSClient.TDS.Utilities;

    /// <summary>
    /// Class describing TDS Client Version.
    /// </summary>
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class TDSClientVersion : IPackageable, IEquatable<TDSClientVersion>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TDSClientVersion" /> class.
        /// </summary>
        public TDSClientVersion() 
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TDSClientVersion" /> class.
        /// </summary>
        /// <param name="major">Major version</param>
        /// <param name="minor">Minor version</param>
        /// <param name="buildNumber">Build number</param>
        /// <param name="subBuildNumber">SubBuild number</param>
        public TDSClientVersion(byte major, byte minor, ushort buildNumber, ushort subBuildNumber)
        {
            this.Major = major;
            this.Minor = minor;
            this.BuildNumber = buildNumber;
            this.SubBuildNumber = subBuildNumber;
        }

        /// <summary>
        /// Gets or sets the Major version
        /// </summary>
        public byte Major { get; set; }

        /// <summary>
        /// Gets or sets the Minor version
        /// </summary>
        public byte Minor { get; set; }

        /// <summary>
        /// Gets or sets the Build number
        /// </summary>
        public ushort BuildNumber { get; set; }

        /// <summary>
        /// Gets or sets the SubBuild number
        /// </summary>
        public ushort SubBuildNumber { get; set; }

        /// <summary>
        /// Used to pack IPackageable to a stream.
        /// </summary>
        /// <param name="stream">MemoryStream in which IPackageable is packet into.</param>
        public void Pack(MemoryStream stream)
        {
            BigEndianUtilities.WriteUShort(stream, this.BuildNumber);
            stream.WriteByte(this.Minor);
            stream.WriteByte(this.Major);
            BigEndianUtilities.WriteUShort(stream, this.SubBuildNumber);
        }

        /// <summary>
        /// Used to unpack IPackageable from a stream.
        /// </summary>
        /// <param name="stream">MemoryStream from which to unpack IPackageable.</param>
        /// <returns>Returns true if successful.</returns>
        public bool Unpack(MemoryStream stream)
        {
            this.BuildNumber = BigEndianUtilities.ReadUShort(stream);
            this.Minor = Convert.ToByte(stream.ReadByte());
            this.Major = Convert.ToByte(stream.ReadByte());
            this.SubBuildNumber = BigEndianUtilities.ReadUShort(stream);
           
            return true;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as TDSClientVersion);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public bool Equals(TDSClientVersion other)
        {
            return other != null &&
                   this.Major == other.Major &&
                   this.Minor == other.Minor &&
                   this.BuildNumber == other.BuildNumber &&
                   this.SubBuildNumber == other.SubBuildNumber;
        }
    }
}