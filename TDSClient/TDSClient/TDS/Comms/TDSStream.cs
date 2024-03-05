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
    public class TDSStream : Stream, IDisposable
    {
        /// <summary>
        /// Gets or sets the Inner Stream.
        /// </summary>
        public Stream InnerStream { get; set; }

        /// <summary>
        /// TDS Packet Size used for communication
        /// </summary>
        private readonly int NegotiatedPacketSize;

        /// <summary>
        /// Current Inbound TDS Packet Header
        /// </summary>
        private TDSPacketHeader CurrentInboundTDSHeader;

        /// <summary>
        /// Current position within the Inbound TDS Packet
        /// </summary>
        private int CurrentInboundPacketPosition;

        /// <summary>
        /// Current Outbound TDS Packet Header
        /// </summary>
        private TDSPacketHeader CurrentOutboundTDSHeader;

        /// <summary>
        /// TDS Connection Timeout
        /// </summary>
        private TimeSpan Timeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="TDSStream"/> class.
        /// </summary>
        /// <param name="innerStream">Inner stream used for communication</param>
        /// <param name="timeout">Communication failure timeout</param>
        /// <param name="negotiatedPacketSize">Packet size</param>
        public TDSStream(Stream innerStream, TimeSpan timeout, int negotiatedPacketSize)
        {
            InnerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));;
            Timeout = timeout;
            NegotiatedPacketSize = negotiatedPacketSize;
        }

        /// <summary>
        /// Gets a value indicating whether inbound message is terminated.
        /// </summary>
        public bool InboundMessageTerminated
        {
            get
            {
                return CurrentInboundTDSHeader == null;
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
        public override bool CanRead => InnerStream.CanRead;

        /// <summary>
        /// Gets or sets CanSeek Flag.
        /// </summary>
        public override bool CanSeek => InnerStream.CanSeek;

        /// <summary>
        /// Gets or sets CanWrite Flag.
        /// </summary>
        public override bool CanWrite => InnerStream.CanWrite;

        /// <summary>
        /// Gets or sets Stream Length.
        /// </summary>
        public override long Length => InnerStream.Length;

        /// <summary>
        /// Gets or sets Stream Position.
        /// </summary>
        public override long Position { get => InnerStream.Position; set => InnerStream.Position = value; }

        /// <summary>
        /// Flushes stream output.
        /// </summary>
        public override void Flush()
        {
            EnsureInnerStreamIsNotNull();
            InnerStream.Flush();
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
            EnsureInnerStreamIsNotNull();

            var startTime = DateTime.Now;
            var bytesReadTotal = 0;

            while (bytesReadTotal < count && DateTime.Now - Timeout < startTime)
            {
                if (CurrentInboundTDSHeader == null || CurrentInboundPacketPosition >= CurrentInboundTDSHeader.ConvertedPacketLength)
                {
                    byte[] headerBuffer = new byte[8];
                    int curPos = 0;
                    do
                    {
                        curPos += InnerStream.Read(headerBuffer, curPos, 8 - curPos);
                        

                        if (curPos == 0)
                        {
                            throw new Exception("Failure to read from network stream.");
                        }
                    } 
                    while (curPos < 8 && DateTime.Now - Timeout < startTime);

                    if (DateTime.Now - Timeout >= startTime)
                    {
                        throw new TimeoutException("Reading from network stream timed out.");
                    }

                    CurrentInboundTDSHeader = new TDSPacketHeader();
                    CurrentInboundTDSHeader.Unpack(new MemoryStream(headerBuffer));
                    CurrentInboundPacketPosition = 8;
                }

                var bytesToReadFromCurrentPacket = Math.Min(count - bytesReadTotal, CurrentInboundTDSHeader.ConvertedPacketLength - CurrentInboundPacketPosition);

                do
                {
                    var bytesRead = InnerStream.Read(buffer, offset + bytesReadTotal, bytesToReadFromCurrentPacket);

                    if (bytesRead == 0)
                    {
                        throw new Exception("Failure to read from network stream.");
                    }

                    bytesToReadFromCurrentPacket -= bytesRead;
                    CurrentInboundPacketPosition += bytesRead;
                    bytesReadTotal += bytesRead;
                } 
                while (bytesToReadFromCurrentPacket > 0 && DateTime.Now - Timeout < startTime);

                if (CurrentInboundTDSHeader != null && CurrentInboundPacketPosition >= CurrentInboundTDSHeader.ConvertedPacketLength && (CurrentInboundTDSHeader.Status & TDSMessageStatus.EndOfMessage) == TDSMessageStatus.EndOfMessage)
                {
                    CurrentInboundTDSHeader = null;
                    return bytesReadTotal;
                }
            }

            if (DateTime.Now - Timeout >= startTime)
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
            EnsureInnerStreamIsNotNull();

            CurrentOutboundTDSHeader = new TDSPacketHeader(CurrentOutboundMessageType, TDSMessageStatus.Normal, 0, 1);

            var bytesSent = 0;

            while (bytesSent < count)
            {
                if (count - bytesSent + 8 < NegotiatedPacketSize)
                {
                    CurrentOutboundTDSHeader.Status = TDSMessageStatus.EndOfMessage;
                }

                var bufferSize = Math.Min(count - bytesSent + 8, NegotiatedPacketSize);
                byte[] packetBuffer = new byte[bufferSize];

                CurrentOutboundTDSHeader.Length = Convert.ToUInt16(bufferSize);
                CurrentOutboundTDSHeader.Pack(new MemoryStream(packetBuffer));
                Array.Copy(buffer, offset + bytesSent, packetBuffer, 8, bufferSize - 8);

                InnerStream.Write(packetBuffer, 0, bufferSize);
                bytesSent += bufferSize - 8;
                
                CurrentOutboundTDSHeader.Packet = (byte)((CurrentOutboundTDSHeader.Packet + 1) % 256);
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
            EnsureInnerStreamIsNotNull();
            return InnerStream.Seek(offset, origin);
        }

        /// <summary>
        /// Set stream length.
        /// </summary>
        /// <param name="value">New length.</param>
        public override void SetLength(long value)
        {
            EnsureInnerStreamIsNotNull();
            InnerStream.SetLength(value);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (InnerStream != null)
                {
                    InnerStream.Dispose();
                    InnerStream = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void EnsureInnerStreamIsNotNull()
        {
            if (InnerStream == null)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
