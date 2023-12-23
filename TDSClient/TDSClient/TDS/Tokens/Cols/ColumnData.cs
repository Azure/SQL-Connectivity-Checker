using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TDSClient.TDS.Interfaces;

namespace TDSClient.TDS.Tokens.Cols
{
    public class ColumnData : IPackageable
    {
        internal ColumnData() { }

        public ulong UserType { get; set; }
        public ushort Flags { get; set; }


        public void Pack(MemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public bool Unpack(MemoryStream stream)
        {
            throw new NotImplementedException();
        }
    }
}
