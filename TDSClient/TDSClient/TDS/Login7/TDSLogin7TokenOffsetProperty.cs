//  ---------------------------------------------------------------------------
//  <copyright file="TDSLogin7TokenOffsetProperty.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Login7
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using TDSClient.TDS.Interfaces;
    using TDSClient.TDS.Utilities;
    using System.Reflection;

    /// <summary>
    /// Helper class that takes care of setting property value
    /// </summary>
    public class TDSLogin7TokenOffsetProperty
    {
        /// <summary>
        /// Property which value is being set
        /// </summary>
        public PropertyInfo Property { get; set; }

        /// <summary>
        /// Position of the value in the data stream
        /// </summary>
        public uint Position { get; set; }

        /// <summary>
        /// Length of the property value in the data stream
        /// </summary>
        public uint Length { get; set; }

        /// <summary>
        /// This property is used to distinguish between "value" position in the stream and "offset of the value" position
        /// </summary>
        public bool IsOffsetOffset { get; set; }

        /// <summary>
        /// Initialization constructor
        /// </summary>
		public TDSLogin7TokenOffsetProperty(PropertyInfo property, ushort position, ushort length)
        {
            Property = property;
            Position = position;
            Length = length;
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
		public TDSLogin7TokenOffsetProperty(PropertyInfo property, ushort position, ushort length, bool isOffsetOffset) :
            this(property, position, length)
        {
            IsOffsetOffset = isOffsetOffset;
        }
    }
}
