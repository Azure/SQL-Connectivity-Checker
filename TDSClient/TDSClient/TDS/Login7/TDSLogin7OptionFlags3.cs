//  ---------------------------------------------------------------------------
//  <copyright file="TDSLogin7OptionFlags3.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Login7
{
    using System;
    using System.IO;
    
    using TDSClient.TDS.Interfaces;

    /// <summary>
    /// Specifies whether the login request SHOULD change password.
    /// </summary>
    public enum TDSLogin7OptionFlags3ChangePassword : byte
    {
        /// <summary>
        /// Change Password Not Requested
        /// </summary>
        NoChangeRequest,

        /// <summary>
        /// Change Password Requested
        /// </summary>
        RequestChange
    }

    /// <summary>
    /// Enum describing SendYukonBinaryXML flag
    /// 1 if XML data type instances are returned as binary XML
    /// </summary>
    public enum TDSLogin7OptionFlags3SendYukonBinaryXML : byte
    {
        /// <summary>
        /// Send Yukon Binary XML Off
        /// </summary>
        Off,

        /// <summary>
        /// Send Yukon Binary XML On
        /// </summary>
        On
    }

    /// <summary>
    /// Enum describing UserInstanceProcess flag
    /// 1 if client is requesting separate process to be spawned as user instance
    /// </summary>
    public enum TDSLogin7OptionFlags3UserInstanceProcess : byte
    {
        /// <summary>
        /// Don't request separate user instance process
        /// </summary>
        DontRequestSeparateProcess,

        /// <summary>
        /// Request separate user instance process
        /// </summary>
        RequestSeparateProcess
    }

    /// <summary>
    /// This bit is used by the server to determine if a client is able to
    /// properly handle collations introduced after TDS 7.2. TDS 7.2 and earlier clients are
    /// encouraged to use this login packet bit.
    /// 0 = The server MUST restrict the collations sent to a specific set of collations.It MAY
    /// disconnect or send an error if some other value is outside the specific collation set.
    /// The client MUST properly support all collations within the collation set.
    /// 1 = The server MAY send any collation that fits in the storage space.The client MUST
    /// be able to both properly support collations and gracefully fail for those it does not
    /// support.
    /// </summary>
    public enum TDSLogin7OptionFlags3UnknownCollationHandling : byte
    {
        /// <summary>
        /// Unknown collation handling off
        /// </summary>
        Off,

        /// <summary>
        /// Unknown collation handling on
        /// </summary>
        On
    }

    /// <summary>
    ///  Specifies whether Extension fields are used.
    /// </summary>
    public enum TDSLogin7OptionFlags3Extension : byte
    {
        /// <summary>
        /// Extension doesn't exist
        /// </summary>
        DoesntExist,

        /// <summary>
        /// Extensions exist and are specified
        /// </summary>
        Exists
    }

    /// <summary>
    /// TDS Login7 Message Option Flags 3
    /// </summary>
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class TDSLogin7OptionFlags3 : IPackageable, IEquatable<TDSLogin7OptionFlags3>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        /// <summary>
        ///  Gets or sets the ChangePassword Flag.
        ///  Specifies whether the login request SHOULD change password.
        /// </summary>
        public TDSLogin7OptionFlags3ChangePassword ChangePassword { get; set; }

        /// <summary>
        /// Gets or sets the SendYukonBinaryXML Flag.
        /// On if XML data type instances are returned as binary XML.
        /// </summary>
        public TDSLogin7OptionFlags3SendYukonBinaryXML SendYukonBinaryXML { get; set; }

        /// <summary>
        /// Gets or sets the UserInstanceProcess Flag.
        /// On if client is requesting separate process to be spawned as user instance.
        /// </summary>
        public TDSLogin7OptionFlags3UserInstanceProcess UserInstanceProcess { get; set; }

        /// <summary>
        /// Gets or sets the UnknownCollationHandling Flag.
        /// This bit is used by the server to determine if a client is able to
        /// properly handle collations introduced after TDS 7.2. 
        /// </summary>
        public TDSLogin7OptionFlags3UnknownCollationHandling UnknownCollationHandling { get; set; }

        /// <summary>
        /// Gets or sets the Extension Flag.
        /// Specifies whether IBExtension or CBExtension fields are used.
        /// </summary>
        public TDSLogin7OptionFlags3Extension Extension { get; set; }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as TDSLogin7OptionFlags3);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public bool Equals(TDSLogin7OptionFlags3 other)
        {
            return other != null &&
                   ChangePassword == other.ChangePassword &&
                   SendYukonBinaryXML == other.SendYukonBinaryXML &&
                   UserInstanceProcess == other.UserInstanceProcess &&
                   UnknownCollationHandling == other.UnknownCollationHandling &&
                   Extension == other.Extension;
        }

        /// <summary>
        /// Used to pack IPackageable to a stream.
        /// </summary>
        /// <param name="stream">MemoryStream in which IPackageable is packet into.</param>
        public void Pack(MemoryStream stream)
        {
            byte packedByte = (byte)((byte)ChangePassword
                | ((byte)UserInstanceProcess << 1)
                | ((byte)SendYukonBinaryXML << 2)
                | ((byte)UnknownCollationHandling << 3)
                | ((byte)Extension << 4));

            stream.WriteByte(packedByte);
        }

        /// <summary>
        /// Used to unpack IPackageable from a stream.
        /// </summary>
        /// <param name="stream">MemoryStream from which to unpack IPackageable.</param>
        /// <returns>Returns true if successful.</returns>
        public bool Unpack(MemoryStream stream)
        {
            byte flagByte = Convert.ToByte(stream.ReadByte());

            ChangePassword = (TDSLogin7OptionFlags3ChangePassword)(flagByte & 0x01);
            UserInstanceProcess = (TDSLogin7OptionFlags3UserInstanceProcess)((flagByte >> 1) & 0x01);
            SendYukonBinaryXML = (TDSLogin7OptionFlags3SendYukonBinaryXML)((flagByte >> 2) & 0x01);
            UnknownCollationHandling = (TDSLogin7OptionFlags3UnknownCollationHandling)((flagByte >> 3) & 0x01);
            Extension = (TDSLogin7OptionFlags3Extension)((flagByte >> 4) & 0x01);
            
            return true;
        }
    }
}
