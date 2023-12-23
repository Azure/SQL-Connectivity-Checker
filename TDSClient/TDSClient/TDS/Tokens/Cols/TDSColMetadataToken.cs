using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TDSClient.TDS.Interfaces;
using TDSClient.TDS.Tokens.Type;
using TDSClient.TDS.Utilities;

namespace TDSClient.TDS.Tokens.Cols
{
    internal class TDSColMetadataToken : TDSToken
    {

        public ushort Count { get; set; }

        public ColMetadata[] Metadata { get; set; }

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
            Count = BigEndianUtilities.ReadUShortLE(stream);

            var metadata = new ColMetadata[Count];
            for (int i = 0; i < Count;i++)
            {
                var colMetadata = new ColMetadata();
                if(!colMetadata.Unpack(stream))
                {
                    return false;
                }

                metadata[i] = colMetadata;
            }

            Metadata = metadata;
            return true;
        }
    }
}
