using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TDSClient.TDS.Interfaces;

namespace TDSClient.TDS.Tokens.Cols
{
    internal class TDSColMetadataToken : TDSToken
    {

        public ushort Count { get; set; }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
}
