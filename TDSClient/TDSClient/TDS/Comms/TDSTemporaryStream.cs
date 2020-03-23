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
            this.InnerStream = innerStream;
        }
     
        /// <summary>
        /// Gets or sets InnerStream.
        /// </summary>
        public Stream InnerStream { get; set; }

        /// <summary>
        /// Gets a value indicating whether you can read from this stream.
        /// </summary>
        public override bool CanRead => this.InnerStream.CanRead;

        /// <summary>
        /// Gets a value indicating whether you can seek throughout this stream.
        /// </summary>
        public override bool CanSeek => this.InnerStream.CanSeek;

        /// <summary>
        /// Gets a value indicating whether you can write to this stream.
        /// </summary>
        public override bool CanWrite => this.InnerStream.CanWrite;

        /// <summary>
        /// Gets the length of this stream, in bytes.
        /// </summary>
        public override long Length => this.InnerStream.Length;

        /// <summary>
        /// Gets the current position within this stream.
        /// </summary>
        public override long Position { get => this.InnerStream.Position; set => this.InnerStream.Position = value; }

        /// <summary>
        /// Flush stream output.
        /// </summary>
        public override void Flush()
        {
            this.InnerStream.Flush();
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
            return this.InnerStream.Read(buffer, offset, count);
        }

        /// <summary>
        /// Seek within stream.
        /// </summary>
        /// <param name="offset">Offset from origin.</param>
        /// <param name="origin">Origin to seek from.</param>
        /// <returns>THe new position within current stream.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.InnerStream.Seek(offset, origin);
        }

        /// <summary>
        /// Set stream length.
        /// </summary>
        /// <param name="value">New length.</param>
        public override void SetLength(long value)
        {
            this.InnerStream.SetLength(value);
        }

        /// <summary>
        /// Write to stream.
        /// </summary>
        /// <param name="buffer">Buffer containing data that's being written.</param>
        /// <param name="offset">Offset within buffer.</param>
        /// <param name="count">Number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            this.InnerStream.Write(buffer, offset, count);
        }
    }
}
