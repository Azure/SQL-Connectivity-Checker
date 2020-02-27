using TDSClient.TDS;
using TDSClient.TDS.Tokens.EnvChange;
using TDSClient.TDS.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TDSClient.TDS.Tokens
{
    public class TDSEnvChangeToken : TDSToken
    {
        public TDSEnvChangeType Type { get; private set; }

        public Dictionary<string, string> Values { get; private set; }

        public TDSEnvChangeToken()
        {
            Values = new Dictionary<string, string>();
        }

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
            var length = LittleEndianUtilities.ReadUShort(stream);
            Type = (TDSEnvChangeType)stream.ReadByte();
            switch (Type)
            {
                case TDSEnvChangeType.Routing:
                    var routingDataValueLength = LittleEndianUtilities.ReadUShort(stream);
                    if (routingDataValueLength == 0 || stream.ReadByte() != 0)
                    {
                        throw new InvalidOperationException();
                    }
                    var protocolProperty = LittleEndianUtilities.ReadUShort(stream);
                    if(protocolProperty == 0)
                    {
                        throw new InvalidOperationException();
                    }

                    int strLength = LittleEndianUtilities.ReadUShort(stream) * 2;

                    var temp = new byte[strLength];
                    stream.Read(temp, 0, strLength);

                    Values["ProtocolProperty"] = string.Format("{0}", protocolProperty);
                    Values["AlternateServer"] = Encoding.Unicode.GetString(temp);

                    for(int i=0; i < length - routingDataValueLength - sizeof(byte) - sizeof(ushort); i++)
                    {
                        // Ignore oldValue
                        stream.ReadByte();
                    }
                    break;
                default:
                    {
                        for (int i = 0; i < length - sizeof(byte); i++)
                        {
                            // Ignore unsupported types
                            stream.ReadByte();
                        }
                        return false;
                    }
            }
            return true;
        }
    }
}
