using System;
using System.Collections.Generic;
using System.IO;
using TDSClient.TDS.Interfaces;

namespace TDSClient.TDS.Tokens
{
    public class TDSTokenStreamPacketData : ITDSPacketData
    {
        public LinkedList<TDSToken> Tokens { get; private set; }

        public TDSTokenStreamPacketData()
        {
            Tokens = new LinkedList<TDSToken>();
        }

        public ushort Length()
        {
            throw new NotImplementedException();
        }

        public void Pack(MemoryStream stream)
        {
            
            throw new NotImplementedException();
        }

        public bool Unpack(MemoryStream stream)
        {
            while (stream.Length > stream.Position)
            {
                TDSToken token = TDSTokenFactory.ReadTokenFromStream(stream);
                if (token != null)
                {
                    Tokens.AddLast(token);
                }
            }
            return true;
        }
    }
}
