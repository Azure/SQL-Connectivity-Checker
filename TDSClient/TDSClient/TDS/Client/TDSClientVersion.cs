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
    public class TDSClientVersion : IPackageable
    {
        /// <summary>
        /// Major version
        /// </summary>
        private byte major;

        /// <summary>
        /// Minor version
        /// </summary>
        private byte minor;

        /// <summary>
        /// Build number
        /// </summary>
        private ushort buildNumber;

        /// <summary>
        /// SubBuild number
        /// </summary>
        private ushort subBuildNumber;

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
            this.major = major;
            this.minor = minor;
            this.buildNumber = buildNumber;
            this.subBuildNumber = subBuildNumber;
        }

        /// <summary>
        /// Used to pack IPackageable to a stream.
        /// </summary>
        /// <param name="stream">MemoryStream in which IPackageable is packet into.</param>
        public void Pack(MemoryStream stream)
        {
            BigEndianUtilities.WriteUShort(stream, this.buildNumber);
            stream.WriteByte(this.minor);
            stream.WriteByte(this.major);
            BigEndianUtilities.WriteUShort(stream, this.subBuildNumber);
        }

        /// <summary>
        /// Used to unpack IPackageable from a stream.
        /// </summary>
        /// <param name="stream">MemoryStream from which to unpack IPackageable.</param>
        /// <returns>Returns true if successful.</returns>
        public bool Unpack(MemoryStream stream)
        {
            this.buildNumber = BigEndianUtilities.ReadUShort(stream);
            this.minor = Convert.ToByte(stream.ReadByte());
            this.major = Convert.ToByte(stream.ReadByte());
            this.subBuildNumber = BigEndianUtilities.ReadUShort(stream);
           
            return true;
        }
    }
}
