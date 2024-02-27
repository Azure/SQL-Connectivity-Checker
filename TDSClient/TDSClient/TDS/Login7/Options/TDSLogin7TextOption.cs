//  ---------------------------------------------------------------------------
//  <copyright file="TDSLogin7TextOption.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Login7.Options
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// TDS Login7 Text Option Type
    /// </summary>
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class TDSLogin7TextOption : TDSLogin7Option, IEquatable<TDSLogin7TextOption>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TDSLogin7TextOption" /> class.
        /// </summary>
        public TDSLogin7TextOption() 
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TDSLogin7TextOption" /> class.
        /// </summary>
        /// <param name="name">Option name</param>
        /// <param name="position">Option position within the packet</param>
        /// <param name="length">Option data length</param>
        public TDSLogin7TextOption(string name, ushort position, ushort length) : base(name, position, length, (ushort)(length * 2))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TDSLogin7TextOption" /> class.
        /// </summary>
        /// <param name="name">Option name</param>
        /// <param name="position">Option position within the packet</param>
        /// <param name="length">Option data length</param>
        /// <param name="text">Text data</param>
        public TDSLogin7TextOption(string name, ushort position, ushort length, string text) : base(name, position, length, (ushort)(length * 2))
        {
            Text = text;
        }

        /// <summary>
        /// Gets or sets option text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as TDSLogin7TextOption);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public bool Equals(TDSLogin7TextOption other)
        {
            if (other == null)
            {
                return false;
            }

            return base.Equals(other) &&
                Text == other.Text;
        }

        /// <summary>
        /// Used to pack IPackageable to a stream.
        /// </summary>
        /// <param name="stream">MemoryStream in which IPackageable is packet into.</param>
        public override void Pack(MemoryStream stream)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(Text);
            stream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Used to unpack IPackageable from a stream.
        /// </summary>
        /// <param name="stream">MemoryStream from which to unpack IPackageable.</param>
        /// <returns>Returns true if successful.</returns>
        public override bool Unpack(MemoryStream stream)
        {
            var buffer = new byte[Length * 2];
            stream.Read(buffer, 0, buffer.Length);
            Text = UnicodeEncoding.Unicode.GetString(buffer);

            return true;
        }
    }
}
