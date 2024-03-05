//  ---------------------------------------------------------------------------
//  <copyright file="TDSLogin7Option.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Login7.Options
{
    using System;
    using System.IO;
    using TDSClient.TDS.Interfaces;

    /// <summary>
    /// TDS Login7 Packet Option
    /// </summary>
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public abstract class TDSLogin7Option : IPackageable, IEquatable<TDSLogin7Option>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TDSLogin7Option" /> class.
        /// </summary>
        public TDSLogin7Option()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TDSLogin7Option" /> class.
        /// </summary>
        /// <param name="name">Option name</param>
        /// <param name="position">Option position within the packet</param>
        /// <param name="length">Option data length</param>
        /// <param name="trueLength">Option data length (in bytes)</param>
        public TDSLogin7Option(string name, ushort position, ushort length, ushort trueLength)
        {
            Name = name;
            Position = position;
            Length = length;
            TrueLength = trueLength;
        }

        /// <summary>
        /// Gets or sets option name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets option position within the packet
        /// </summary>
        public ushort Position { get; set; }

        /// <summary>
        /// Gets or sets option data length
        /// </summary>
        public ushort Length { get; set; }

        /// <summary>
        /// Gets or sets option true data length (in bytes)
        /// </summary>
        public ushort TrueLength { get; set; }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as TDSLogin7Option);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public bool Equals(TDSLogin7Option other)
        {
            if (other == null)
            {
                return false;
            }

            return Name == other.Name &&
               Position == other.Position &&
               Length == other.Length &&
               TrueLength == other.TrueLength;
        }

        /// <summary>
        /// Used to pack IPackageable to a stream.
        /// </summary>
        /// <param name="stream">MemoryStream in which IPackageable is packet into.</param>
        public abstract void Pack(MemoryStream stream);

        /// <summary>
        /// Used to unpack IPackageable from a stream.
        /// </summary>
        /// <param name="stream">MemoryStream from which to unpack IPackageable.</param>
        /// <returns>Returns true if successful.</returns>
        public abstract bool Unpack(MemoryStream stream);
    }
}
