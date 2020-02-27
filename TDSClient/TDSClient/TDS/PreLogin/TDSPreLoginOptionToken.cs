using System;
using System.IO;
using TDSClient.TDS.Interfaces;
using TDSClient.TDS.Utilities;

namespace TDSClient.TDS.PreLogin
{
    public class TDSPreLoginOptionToken : IPackageable
    {
        public TDSPreLoginOptionTokenType Type { get; private set; }
        public ushort Offset { get; set; }
        public ushort Length { get; private set; }

        public TDSPreLoginOptionToken() {}

        public TDSPreLoginOptionToken(TDSPreLoginOptionTokenType type)
        {
            Type = type;
            switch (Type)
            {
                case TDSPreLoginOptionTokenType.Encryption:
                    {
                        Length = 1;
                        break;
                    }
                case TDSPreLoginOptionTokenType.FedAuthRequired:
                    {
                        Length = 1;
                        break;
                    }
                case TDSPreLoginOptionTokenType.InstOpt:
                    {
                        throw new NotSupportedException();
                    }
                case TDSPreLoginOptionTokenType.MARS:
                    {
                        Length = 1;
                        break;
                    }
                case TDSPreLoginOptionTokenType.NonceOpt:
                    {
                        Length = 32;
                        break;
                    }
                case TDSPreLoginOptionTokenType.Terminator:
                    { 
                        Length = 0;
                        break;
                    }
                case TDSPreLoginOptionTokenType.ThreadID:
                    { 
                        Length = 4;
                        break;
                    }
                case TDSPreLoginOptionTokenType.TraceID:
                    {
                        Length = 36;
                        break;
                    }
                case TDSPreLoginOptionTokenType.Version:
                    {  
                        Length = 6;
                        break;
                    }
            }
        }

        public void Pack(MemoryStream stream)
        {
            stream.WriteByte((byte)Type);
            if (Type != TDSPreLoginOptionTokenType.Terminator)
            {
                BigEndianUtilities.WriteUShort(stream, Offset);
                BigEndianUtilities.WriteUShort(stream, Length);
            }
        }

        public bool Unpack(MemoryStream stream)
        {
            Type = (TDSPreLoginOptionTokenType)stream.ReadByte();

            if (Type != TDSPreLoginOptionTokenType.Terminator)
            {
                Offset = BigEndianUtilities.ReadUShort(stream);
                Length = BigEndianUtilities.ReadUShort(stream);
            }
            return true;
        }
    }
}
