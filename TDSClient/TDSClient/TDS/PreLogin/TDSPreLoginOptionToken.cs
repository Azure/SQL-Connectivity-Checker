//  ---------------------------------------------------------------------------
//  <copyright file="TDSPreLoginOptionToken.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.PreLogin
{
    using System;
    using System.IO;
    using TDSClient.TDS.Interfaces;
    using TDSClient.TDS.Utilities;

    public class TDSPreLoginOptionToken : IPackageable
    {
        public TDSPreLoginOptionToken()
        {
        }

        public TDSPreLoginOptionToken(TDSPreLoginOptionTokenType type)
        {
            this.Type = type;
            switch (this.Type)
            {
                case TDSPreLoginOptionTokenType.Encryption:
                    {
                        this.Length = 1;
                        break;
                    }

                case TDSPreLoginOptionTokenType.FedAuthRequired:
                    {
                        this.Length = 1;
                        break;
                    }

                case TDSPreLoginOptionTokenType.InstOpt:
                    {
                        throw new NotSupportedException();
                    }

                case TDSPreLoginOptionTokenType.MARS:
                    {
                        this.Length = 1;
                        break;
                    }

                case TDSPreLoginOptionTokenType.NonceOpt:
                    {
                        this.Length = 32;
                        break;
                    }

                case TDSPreLoginOptionTokenType.Terminator:
                    {
                        this.Length = 0;
                        break;
                    }

                case TDSPreLoginOptionTokenType.ThreadID:
                    {
                        this.Length = 4;
                        break;
                    }

                case TDSPreLoginOptionTokenType.TraceID:
                    {
                        this.Length = 36;
                        break;
                    }

                case TDSPreLoginOptionTokenType.Version:
                    {
                        this.Length = 6;
                        break;
                    }
            }
        }

        public TDSPreLoginOptionTokenType Type { get; private set; }

        public ushort Offset { get; set; }

        public ushort Length { get; private set; }

        public void Pack(MemoryStream stream)
        {
            stream.WriteByte((byte)this.Type);
            if (this.Type != TDSPreLoginOptionTokenType.Terminator)
            {
                BigEndianUtilities.WriteUShort(stream, this.Offset);
                BigEndianUtilities.WriteUShort(stream, this.Length);
            }
        }

        public bool Unpack(MemoryStream stream)
        {
            this.Type = (TDSPreLoginOptionTokenType)stream.ReadByte();

            if (this.Type != TDSPreLoginOptionTokenType.Terminator)
            {
                this.Offset = BigEndianUtilities.ReadUShort(stream);
                this.Length = BigEndianUtilities.ReadUShort(stream);
            }

            return true;
        }
    }
}
