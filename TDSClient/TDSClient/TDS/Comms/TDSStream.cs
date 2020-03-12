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

    public class TDSStream : Stream
    {
        public Stream InnerStream { get; set; }

        public TDSPacketHeader CurrentInboundTDSHeader { get; private set; }

        public TDSMessageType LastInboundTDSMessageType { get; private set; }

        public bool InboundMessageTerminated
        {
            get
            {
                return CurrentInboundTDSHeader == null;
            }
        }

        public TDSPacketHeader CurrentOutboundTDSHeader { get; private set; }

        public TDSMessageType CurrentOutboundMessageType { get; set; }

        public ushort CurrentOutboundMessageSPID { get; set; }

        public TimeSpan Timeout { get; private set; }

        public int NegotiatedPacketSize { get; private set; }

        private int CurrentInboundPacketPosition { get; set; }

        public override bool CanTimeout => true;

        public override bool CanRead => InnerStream.CanRead;

        public override bool CanSeek => InnerStream.CanSeek;

        public override bool CanWrite => InnerStream.CanWrite;

        public override long Length => InnerStream.Length;

        public override long Position { get => InnerStream.Position; set => InnerStream.Position = value; }

        public TDSStream(Stream innerStream, TimeSpan timeout, int negotiatedPacketSize)
        {
            InnerStream = innerStream;
            Timeout = timeout;
            NegotiatedPacketSize = negotiatedPacketSize;
        }

        public override void Flush()
        {
            InnerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
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
                    LastInboundTDSMessageType = CurrentInboundTDSHeader.Type;
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

        public override void Write(byte[] buffer, int offset, int count)
        {
            CurrentOutboundTDSHeader = new TDSPacketHeader(CurrentOutboundMessageType, TDSMessageStatus.Normal, CurrentOutboundMessageSPID, 1);

            var bytesSent = 0;

            while (bytesSent < count)
            {
                if (count - bytesSent - 8 < NegotiatedPacketSize)
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

        public override long Seek(long offset, SeekOrigin origin)
        {
            return InnerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            InnerStream.SetLength(value);
        }

        public override void Close()
        {
            InnerStream.Close();
            base.Close();
        }
    }
}
