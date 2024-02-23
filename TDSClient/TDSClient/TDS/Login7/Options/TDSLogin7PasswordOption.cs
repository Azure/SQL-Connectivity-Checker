//  ---------------------------------------------------------------------------
//  <copyright file="TDSLogin7PasswordOption.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Login7.Options
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// TDS Login7 Password Option Type
    /// </summary>
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class TDSLogin7PasswordOption : TDSLogin7Option, IEquatable<TDSLogin7PasswordOption>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TDSLogin7PasswordOption" /> class.
        /// </summary>
        public TDSLogin7PasswordOption()
        { 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TDSLogin7PasswordOption" /> class.
        /// </summary>
        /// <param name="name">Option name</param>
        /// <param name="position">Option position within the packet</param>
        /// <param name="length">Option data length</param>
        public TDSLogin7PasswordOption(string name, ushort position, ushort length) : base(name, position, length, (ushort)(length * 2))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TDSLogin7PasswordOption" /> class.
        /// </summary>
        /// <param name="name">Option name</param>
        /// <param name="position">Option position within the packet</param>
        /// <param name="length">Option data length</param>
        /// <param name="plainTextPassword">Password in plain text</param>
        public TDSLogin7PasswordOption(string name, ushort position, ushort length, string plainTextPassword) : base(name, position, length, (ushort)(length * 2))
        {
            this.PlainTextPassword = plainTextPassword;
        }

        /// <summary>
        /// Gets or sets password in plain text
        /// </summary>
        public string PlainTextPassword { get; set; }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as TDSLogin7PasswordOption);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public bool Equals(TDSLogin7PasswordOption other)
        {
            return other != null &&
                   base.Equals(other) &&
                   this.PlainTextPassword == other.PlainTextPassword;
        }

        /// <summary>
        /// Used to pack IPackageable to a stream.
        /// </summary>
        /// <param name="stream">MemoryStream in which IPackageable is packet into.</param>
        public override void Pack(MemoryStream stream)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(this.PlainTextPassword);
            buffer = this.GetScrambledPassword(buffer);
            stream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Used to unpack IPackageable from a stream.
        /// </summary>
        /// <param name="stream">MemoryStream from which to unpack IPackageable.</param>
        /// <returns>Returns true if successful.</returns>
        public override bool Unpack(MemoryStream stream)
        {
            var buffer = new byte[this.Length * 2];
            stream.Read(buffer, 0, buffer.Length);
            buffer = this.GetUnscrambledPassword(buffer);
            this.PlainTextPassword = UnicodeEncoding.Unicode.GetString(buffer);

            return true;
        }

        /// <summary>
        /// Scrambles password bytes in a way described in TDS documentation
        /// </summary>
        /// <param name="password">Password bytes</param>
        /// <returns>Scrambled password bytes</returns>
        private byte[] GetScrambledPassword(byte[] password)
        {
            for (int i = 0; i < password.Length; i++)
            {
                var piece0 = (byte)(password[i] >> 4);
                var piece1 = (byte)(password[i] & 0x0f);
                password[i] = (byte)((piece0 | (piece1 << 4)) ^ 0xA5);
            }

            return password;
        }

        /// <summary>
        /// Unscrambles password bytes in a way described in TDS documentation
        /// </summary>
        /// <param name="password">Scrambled password bytes</param>
        /// <returns>Unscrambled password bytes</returns>
        private byte[] GetUnscrambledPassword(byte[] password)
        {
            for (int i = 0; i < password.Length; i++)
            {
                password[i] = (byte)(password[i] ^ 0xA5);
                password[i] = (byte)((password[i] >> 4) | (password[i] & 0x0f));
            }

            return password;
        }
    }
}
