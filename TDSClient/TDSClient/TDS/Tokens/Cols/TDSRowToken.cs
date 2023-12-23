using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using TDSClient.TDS.Interfaces;
using TDSClient.TDS.Tokens.Type;
using TDSClient.TDS.Utilities;

namespace TDSClient.TDS.Tokens.Cols
{
    internal class TDSRowToken : TDSToken
    {
        public TDSRowToken(TDSColMetadataToken colMetadata)
        {
            ColMetadata = colMetadata;
        }

        public object[] Values { get; set; } = null;
        public TDSColMetadataToken ColMetadata { get; }

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
            Values = new object[ColMetadata.Count];
            for (int i = 0; i < ColMetadata.Count; i++)
            {
                var type = ColMetadata.Metadata[i].Type;
                Values[i] = SqlTypeValueFactory.ReadVaue(stream, type);
            }

            return true;
        }
    }
}
