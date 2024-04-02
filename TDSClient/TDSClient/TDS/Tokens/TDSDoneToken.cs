using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TDSClient.TDS.Tokens;
using TDSClient.TDS.Utilities;

namespace TDSClient.TDS.Tokens
{
    /// <summary>
    /// Status of the token
    /// </summary>
    public enum TDSDoneTokenStatusType : ushort
    {
        Final = 0x00,
        More = 0x01,
        Error = 0x02,
        TransactionInProgress = 0x04,
        Count = 0x10,
        Attention = 0x20,
        ServerError = 0x100
    }

    /// <summary>
    /// Completion Done token.
    /// </summary>
    public class TDSDoneToken : TDSToken
    {
        /// <summary>
        /// Status of the completion
        /// </summary>
        public TDSDoneTokenStatusType Status { get; set; }

        /// <summary>
        /// Token for which completion is indicated
        /// </summary>
        public ushort Command { get; set; }

        /// <summary>
        /// Amount of rows returned
        /// </summary>
        public ulong RowCount { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public TDSDoneToken()
        {
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        public TDSDoneToken(TDSDoneTokenStatusType status)
        {
            Status = status;
        }

        /// <summary>
        /// Inflate the token
        /// NOTE: This operation is not continuable and assumes that the entire token is available in the stream
        /// </summary>
        /// <param name="source">Stream to inflate the token from</param>
        /// <returns>TRUE if inflation is complete</returns>
        public override bool Unpack(MemoryStream source)
        {
            Status = (TDSDoneTokenStatusType)LittleEndianUtilities.ReadUShort(source);
            Command = LittleEndianUtilities.ReadUShort(source);
            RowCount = LittleEndianUtilities.ReadULong(source);

            return true;
        }

        /// <summary>
        /// Deflate the token
        /// </summary>
        /// <param name="destination">Stream to deflate token to</param>
        public override void Pack(MemoryStream destination)
        {
            destination.WriteByte((byte)TDSTokenType.Done);
            LittleEndianUtilities.WriteUShort(destination, (ushort)Status);
            LittleEndianUtilities.WriteUShort(destination, (ushort)Command);
            LittleEndianUtilities.WriteULong(destination, RowCount);
        }

        public override ushort Length()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        public override void ProcessToken()
        {
            LoggingUtilities.WriteLog($"  Processing Done token:");
            LoggingUtilities.WriteLog($"     Status: {Status}");
            LoggingUtilities.WriteLog($"     Current SQL statement command: {Command}");
            LoggingUtilities.WriteLog($"     Done Row Count: {RowCount}");
        }
    }
}
