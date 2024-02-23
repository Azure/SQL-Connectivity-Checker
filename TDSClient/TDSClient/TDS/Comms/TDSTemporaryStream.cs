//  ---------------------------------------------------------------------------
//  <copyright file="TDSTemporaryStream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Comms
{
    using System.IO;

    /// <summary>
    /// Stream used for enabling TLS through TDS.
    /// </summary>
    public class TDSTemporaryStream : Stream
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TDSTemporaryStream"/> class.
        /// </summary>
        /// <param name="innerStream">Inner stream used for communication</param>
        public TDSTemporaryStream(Stream innerStream)
        {
            InnerStream = innerStream;
        }
     
        /// <summary>
        /// Gets or sets InnerStream.
        /// </summary>
        public Stream InnerStream { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether you can read from this stream.
        /// </summary>
        public override bool CanRead => InnerStream.CanRead;

        /// <summary>
        /// Gets or sets a value indicating whether you can seek throughout this stream.
        /// </summary>
        public override bool CanSeek => InnerStream.CanSeek;

        /// <summary>
        /// Gets or sets a value indicating whether you can write to this stream.
        /// </summary>
        public override bool CanWrite => InnerStream.CanWrite;

        /// <summary>
        /// Gets or sets the length of this stream, in bytes.
        /// </summary>
        public override long Length => InnerStream.Length;

        /// <summary>
        /// Gets or sets the current position within this stream.
        /// </summary>
        public override long Position { get => InnerStream.Position; set => InnerStream.Position = value; }

        /// <summary>
        /// Flush stream output.
        /// </summary>
        public override void Flush()
        {
            InnerStream.Flush();
        }

        /// <summary>
        /// Read from inner stream.
        /// </summary>
        /// <param name="buffer">Buffer to store read data to.</param>
        /// <param name="offset">Offset within buffer.</param>
        /// <param name="count">Number of bytes to read.</param>
        /// <returns>Returns number of successfully read bytes.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return InnerStream.Read(buffer, offset, count);
        }

        /// <summary>
        /// Seek within stream.
        /// </summary>
        /// <param name="offset">Offset from origin.</param>
        /// <param name="origin">Origin to seek from.</param>
        /// <returns>THe new position within current stream.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return InnerStream.Seek(offset, origin);
        }

        /// <summary>
        /// Set stream length.
        /// </summary>
        /// <param name="value">New length.</param>
        public override void SetLength(long value)
        {
            InnerStream.SetLength(value);
        }

        /// <summary>
        /// Write to stream.
        /// </summary>
        /// <param name="buffer">Buffer containing data that's being written.</param>
        /// <param name="offset">Offset within buffer.</param>
        /// <param name="count">Number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            InnerStream.Write(buffer, offset, count);
        }
    }
}
