//  ---------------------------------------------------------------------------
//  <copyright file="TDSTemporaryStream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Comms
{
    using System.IO;

    public class TDSTemporaryStream : Stream
    {
        public TDSTemporaryStream(Stream innerStream)
        {
            this.InnerStream = innerStream;
        }
     
        /// <summary>
        /// Gets or sets InnerStream.
        /// </summary>
        public Stream InnerStream { get; set; }

        public override bool CanRead => this.InnerStream.CanRead;

        public override bool CanSeek => this.InnerStream.CanSeek;

        public override bool CanWrite => this.InnerStream.CanWrite;

        public override long Length => this.InnerStream.Length;

        public override long Position { get => this.InnerStream.Position; set => this.InnerStream.Position = value; }

        public override void Flush()
        {
            this.InnerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.InnerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.InnerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.InnerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.InnerStream.Write(buffer, offset, count);
        }
    }
}
