//  ---------------------------------------------------------------------------
//  <copyright file="TDSInfoToken.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Tokens
{
    using System;
    using System.IO;
    using System.Text;
    
    using TDSClient.TDS.Utilities;

    /// <summary>
    /// Class describing TDS Information Token
    /// </summary>
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class TDSInfoToken : TDSToken, IEquatable<TDSInfoToken>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        /// <summary>
        /// Gets or sets information message number.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Gets or sets information message state.
        /// </summary>
        public byte State { get; set; }

        /// <summary>
        /// Gets or sets information message class.
        /// </summary>
        public byte Class { get; set; }

        /// <summary>
        /// Gets or sets information message.
        /// </summary>
        public string MsgText { get; set; }

        /// <summary>
        /// Gets or sets server name.
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// Gets or sets stored procedure name.
        /// </summary>
        public string ProcName { get; set; }

        /// <summary>
        /// Gets or sets line number.
        /// </summary>
        public uint LineNumber { get; set; }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as TDSInfoToken);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public bool Equals(TDSInfoToken other)
        {
            return other != null &&
                   Number == other.Number &&
                   State == other.State &&
                   Class == other.Class &&
                   MsgText == other.MsgText &&
                   ServerName == other.ServerName &&
                   ProcName == other.ProcName &&
                   LineNumber == other.LineNumber;
        }

        /// <summary>
        /// TDS Error Token Length
        /// </summary>
        /// <returns>Returns TDS Error Token Length</returns>
        public override ushort Length()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Used to pack IPackageable to a stream.
        /// </summary>
        /// <param name="stream">MemoryStream in which IPackageable is packet into.</param>
        public override void Pack(MemoryStream stream)
        {
            throw new NotImplementedException();
        }

        public override void ProcessToken()
        {
            LoggingUtilities.WriteLog($"  Client received Info token:");
            LoggingUtilities.WriteLog($"     Number: {Number}");
            LoggingUtilities.WriteLog($"     State: {State}");
            LoggingUtilities.WriteLog($"     Class: {Class}");
            LoggingUtilities.WriteLog($"     MsgText: {MsgText}");
            LoggingUtilities.WriteLog($"     ServerName: {ServerName}");
            LoggingUtilities.WriteLog($"     ProcName: {ProcName}");
            LoggingUtilities.WriteLog($"     LineNumber: {LineNumber}");
        }

        /// <summary>
        /// Used to unpack IPackageable from a stream.
        /// </summary>
        /// <param name="stream">MemoryStream from which to unpack IPackageable.</param>
        /// <returns>Returns true if successful.</returns>
        public override bool Unpack(MemoryStream stream)
        {
            LittleEndianUtilities.ReadUShort(stream);
            Number = (int)LittleEndianUtilities.ReadUInt(stream);
            State = Convert.ToByte(stream.ReadByte());
            Class = Convert.ToByte(stream.ReadByte());

            int length = LittleEndianUtilities.ReadUShort(stream) * 2;
            var buffer = new byte[length];
            stream.Read(buffer, 0, length);
            MsgText = Encoding.Unicode.GetString(buffer);

            length = stream.ReadByte() * 2;
            buffer = new byte[length];
            stream.Read(buffer, 0, length);
            ServerName = Encoding.Unicode.GetString(buffer);

            length = stream.ReadByte() * 2;
            buffer = new byte[length];
            stream.Read(buffer, 0, length);
            ProcName = Encoding.Unicode.GetString(buffer);

            LineNumber = LittleEndianUtilities.ReadUInt(stream);

            return true;
        }
    }
}
