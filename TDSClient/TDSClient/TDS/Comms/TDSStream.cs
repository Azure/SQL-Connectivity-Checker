//  ---------------------------------------------------------------------------
//  <copyright file="TDSStream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Comms
{
    using System;
    using System.IO;
    using TDSClient.TDS.Header;

    /// <summary>
    /// Stream used to pass TDS messages.
    /// </summary>
    public class TDSStream : Stream
    {
        /// <summary>
        /// TDS Packet Size used for communication
        /// </summary>
        private readonly int negotiatedPacketSize;

        /// <summary>
        /// Current Inbound TDS Packet Header
        /// </summary>
        private TDSPacketHeader currentInboundTDSHeader;

        /// <summary>
        /// Current position within the Inbound TDS Packet
        /// </summary>
        private int currentInboundPacketPosition;

        /// <summary>
        /// Current Outbound TDS Packet Header
        /// </summary>
        private TDSPacketHeader currentOutboundTDSHeader;

        /// <summary>
        /// TDS Connection Timeout
        /// </summary>
        private TimeSpan timeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="TDSStream"/> class.
        /// </summary>
        /// <param name="innerStream">Inner stream used for communication</param>
        /// <param name="timeout">Communication failure timeout</param>
        /// <param name="negotiatedPacketSize">Packet size</param>
        public TDSStream(Stream innerStream, TimeSpan timeout, int negotiatedPacketSize)
        {
            this.InnerStream = innerStream;
            this.timeout = timeout;
            this.negotiatedPacketSize = negotiatedPacketSize;
        }

        /// <summary>
        /// Gets or sets the Inner Stream.
        /// </summary>
        public Stream InnerStream { get; set; }

        /// <summary>
        /// Gets a value indicating whether inbound message is terminated.
        /// </summary>
        public bool InboundMessageTerminated
        {
            get
            {
                return this.currentInboundTDSHeader == null;
            }
        }
        
        /// <summary>
        /// Gets or sets the current outbound message type.
        /// </summary>
        public TDSMessageType CurrentOutboundMessageType { get; set; }

        /// <summary>
        /// Gets or sets CanTimeout Flag.
        /// </summary>
        public override bool CanTimeout => true;

        /// <summary>
        /// Gets or sets CanRead Flag.
        /// </summary>
        public override bool CanRead => this.InnerStream.CanRead;

        /// <summary>
        /// Gets or sets CanSeek Flag.
        /// </summary>
        public override bool CanSeek => this.InnerStream.CanSeek;

        /// <summary>
        /// Gets or sets CanWrite Flag.
        /// </summary>
        public override bool CanWrite => this.InnerStream.CanWrite;

        /// <summary>
        /// Gets or sets Stream Length.
        /// </summary>
        public override long Length => this.InnerStream.Length;

        /// <summary>
        /// Gets or sets Stream Position.
        /// </summary>
        public override long Position { get => this.InnerStream.Position; set => this.InnerStream.Position = value; }

        /// <summary>
        /// Flushes stream output.
        /// </summary>
        public override void Flush()
        {
            this.InnerStream.Flush();
        }

        /// <summary>
        /// Reads from stream.
        /// </summary>
        /// <param name="buffer">Buffer used to store read data.</param>
        /// <param name="offset">Offset within buffer.</param>
        /// <param name="count">Number of bytes to read.</param>
        /// <returns>Returns number of successfully read bytes.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var startTime = DateTime.Now;
            var bytesReadTotal = 0;

            while (bytesReadTotal < count && DateTime.Now - this.timeout < startTime)
            {
                if (this.currentInboundTDSHeader == null || this.currentInboundPacketPosition >= this.currentInboundTDSHeader.ConvertedPacketLength)
                {
                    byte[] headerBuffer = new byte[8];
                    int curPos = 0;
                    do
                    {
                        curPos += this.InnerStream.Read(headerBuffer, curPos, 8 - curPos);

                        if (curPos == 0)
                        {
                            throw new Exception("Failure to read from network stream.");
                        }
                    } 
                    while (curPos < 8 && DateTime.Now - this.timeout < startTime);

                    if (DateTime.Now - this.timeout >= startTime)
                    {
                        throw new TimeoutException("Reading from network stream timed out.");
                    }

                    this.currentInboundTDSHeader = new TDSPacketHeader();
                    this.currentInboundTDSHeader.Unpack(new MemoryStream(headerBuffer));
                    this.currentInboundPacketPosition = 8;
                }

                var bytesToReadFromCurrentPacket = Math.Min(count - bytesReadTotal, this.currentInboundTDSHeader.ConvertedPacketLength - this.currentInboundPacketPosition);

                do
                {
                    var bytesRead = this.InnerStream.Read(buffer, offset + bytesReadTotal, bytesToReadFromCurrentPacket);

                    if (bytesRead == 0)
                    {
                        throw new Exception("Failure to read from network stream.");
                    }

                    bytesToReadFromCurrentPacket -= bytesRead;
                    this.currentInboundPacketPosition += bytesRead;
                    bytesReadTotal += bytesRead;
                } 
                while (bytesToReadFromCurrentPacket > 0 && DateTime.Now - this.timeout < startTime);

                if (this.currentInboundTDSHeader != null && this.currentInboundPacketPosition >= this.currentInboundTDSHeader.ConvertedPacketLength && (this.currentInboundTDSHeader.Status & TDSMessageStatus.EndOfMessage) == TDSMessageStatus.EndOfMessage)
                {
                    this.currentInboundTDSHeader = null;
                    return bytesReadTotal;
                }
            }

            if (DateTime.Now - this.timeout >= startTime)
            {
                throw new TimeoutException("Reading from network stream timed out.");
            }

            return bytesReadTotal;
        }

        /// <summary>
        /// Write to stream.
        /// </summary>
        /// <param name="buffer">Buffer containing data that's being written.</param>
        /// <param name="offset">Offset within buffer.</param>
        /// <param name="count">Number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            this.currentOutboundTDSHeader = new TDSPacketHeader(this.CurrentOutboundMessageType, TDSMessageStatus.Normal, 0, 1);

            var bytesSent = 0;

            while (bytesSent < count)
            {
                if (count - bytesSent + 8 < this.negotiatedPacketSize)
                {
                    this.currentOutboundTDSHeader.Status = TDSMessageStatus.EndOfMessage;
                }

                var bufferSize = Math.Min(count - bytesSent + 8, this.negotiatedPacketSize);
                byte[] packetBuffer = new byte[bufferSize];

                this.currentOutboundTDSHeader.Length = Convert.ToUInt16(bufferSize);
                this.currentOutboundTDSHeader.Pack(new MemoryStream(packetBuffer));
                Array.Copy(buffer, offset + bytesSent, packetBuffer, 8, bufferSize - 8);

                this.InnerStream.Write(packetBuffer, 0, bufferSize);
                bytesSent += bufferSize - 8;

                this.currentOutboundTDSHeader.Packet = (byte)((this.currentOutboundTDSHeader.Packet + 1) % 256);
            }
        }

        /// <summary>
        /// Seek within stream.
        /// </summary>
        /// <param name="offset">Offset from origin.</param>
        /// <param name="origin">Origin to seek from.</param>
        /// <returns>The new position within current stream.</returns>
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
        /// Close this stream.
        /// </summary>
        public override void Close()
        {
            this.InnerStream.Close();
            base.Close();
        }
    }
}
