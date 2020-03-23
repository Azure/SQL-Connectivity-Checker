//  ---------------------------------------------------------------------------
//  <copyright file="TDSErrorToken.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Tokens
{
    using System;
    using System.IO;
    using System.Text;
    using TDSClient.TDS.Utilities;

    public class TDSErrorToken : TDSToken
    {
        public int Number { get; private set; }

        public byte State { get; private set; }

        public byte Class { get; private set; }

        public string MsgText { get; private set; }

        public string ServerName { get; private set; }

        public string ProcName { get; private set; }

        public uint LineNumber { get; private set; }

        public override ushort Length()
        {
            throw new NotImplementedException();
        }

        public override void Pack(MemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public override bool Unpack(MemoryStream stream)
        {
            LittleEndianUtilities.ReadUShort(stream);
            this.Number = (int)LittleEndianUtilities.ReadUInt(stream);
            this.State = Convert.ToByte(stream.ReadByte());
            this.Class = Convert.ToByte(stream.ReadByte());

            int length = LittleEndianUtilities.ReadUShort(stream) * 2;
            var buffer = new byte[length];
            stream.Read(buffer, 0, length);
            this.MsgText = Encoding.Unicode.GetString(buffer);

            length = stream.ReadByte() * 2;
            buffer = new byte[length];
            stream.Read(buffer, 0, length);
            this.ServerName = Encoding.Unicode.GetString(buffer);

            length = stream.ReadByte() * 2;
            buffer = new byte[length];
            stream.Read(buffer, 0, length);
            this.ProcName = Encoding.Unicode.GetString(buffer);

            this.LineNumber = LittleEndianUtilities.ReadUInt(stream);

            return true;
        }
    }
}
