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
        private byte Major;
        private byte Minor;
        private ushort BuildNumber;
        private ushort SubBuildNumber;

        public TDSClientVersion() 
        {
        }

        public TDSClientVersion(byte major, byte minor, ushort buildNumber, ushort subBuildNumber)
        {
            Major = major;
            Minor = minor;
            BuildNumber = buildNumber;
            SubBuildNumber = subBuildNumber;
        }

        public void Pack(MemoryStream stream)
        {
            BigEndianUtilities.WriteUShort(stream, BuildNumber);
            stream.WriteByte(Minor);
            stream.WriteByte(Major);
            BigEndianUtilities.WriteUShort(stream, SubBuildNumber);
        }

        public bool Unpack(MemoryStream stream)
        {
            BuildNumber = BigEndianUtilities.ReadUShort(stream);
            Minor = Convert.ToByte(stream.ReadByte());
            Major = Convert.ToByte(stream.ReadByte());
            SubBuildNumber = BigEndianUtilities.ReadUShort(stream);
            return true;
        }
    }
}
