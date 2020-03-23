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

    public class TDSClientVersion : IPackageable
    {
        private byte major;
        private byte minor;
        private ushort buildNumber;
        private ushort subBuildNumber;

        public TDSClientVersion() 
        {
        }

        public TDSClientVersion(byte major, byte minor, ushort buildNumber, ushort subBuildNumber)
        {
            this.major = major;
            this.minor = minor;
            this.buildNumber = buildNumber;
            this.subBuildNumber = subBuildNumber;
        }

        public void Pack(MemoryStream stream)
        {
            BigEndianUtilities.WriteUShort(stream, this.buildNumber);
            stream.WriteByte(this.minor);
            stream.WriteByte(this.major);
            BigEndianUtilities.WriteUShort(stream, this.subBuildNumber);
        }

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
