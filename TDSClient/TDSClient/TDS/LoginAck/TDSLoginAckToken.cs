using System;
using System.IO;
using System.Text;

using TDSClient.TDS.Login7;
using TDSClient.TDS.Tokens;

namespace TDSClient.TDS.LoginAck
{
    /// <summary>
    /// Login acknowledgement packet
    /// </summary>
    #pragma warning disable CS0659
    public class TDSLoginAckToken : TDSToken
    #pragma warning restore CS0659
    {
        /// <summary>
        /// TDS Version used by the server
        /// </summary>
        public Version TDSVersion { get; set; }

        /// <summary>
        /// The type of interface with which the server will accept client requests
        /// </summary>
        public TDSLogin7TypeFlagsSQLType Interface { get; set; }

        /// <summary>
        /// Name of the server (e.g. "Microsoft SQL Server")
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// Server version
        /// </summary>
        public Version ServerVersion { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public TDSLoginAckToken()
        {
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        public TDSLoginAckToken(Version serverVersion)
        {
            ServerVersion = serverVersion;
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        public TDSLoginAckToken(Version serverVersion, Version tdsVersion) :
            this(serverVersion)
        {
            TDSVersion = tdsVersion;
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        public TDSLoginAckToken(Version serverVersion, Version tdsVersion, TDSLogin7TypeFlagsSQLType interfaceFlags) :
            this(serverVersion, tdsVersion)
        {
            Interface = interfaceFlags;
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        public TDSLoginAckToken(Version serverVersion, Version tdsVersion, TDSLogin7TypeFlagsSQLType interfaceFlags, string serverName) :
            this(serverVersion, tdsVersion, interfaceFlags)
        {
            ServerName = serverName;
        }

        /// <summary>
        /// Unpack the token
        /// NOTE: This operation is not continuable and assumes that the entire token is available in the stream
        /// </summary>
        /// <param name="source">Stream to inflate the token from</param>
        public override bool Unpack(MemoryStream source)
        {
            // We skip the token identifier because it is read by token factory

            ushort tokenLength = (ushort)(source.ReadByte() + (source.ReadByte() << 8));

            Interface = (TDSLogin7TypeFlagsSQLType)source.ReadByte();

            string tdsVersion = String.Format("{0:X}", (uint)(source.ReadByte() << 24)
                + (uint)(source.ReadByte() << 16)
                + (uint)(source.ReadByte() << 8)
                + (uint)(source.ReadByte()));

            // Consturct TDS version
            // See tds.h for TDSXX clarifications
            TDSVersion = new Version(int.Parse(tdsVersion.Substring(0, 1)), int.Parse(tdsVersion.Substring(1, 1)), Convert.ToInt32(tdsVersion.Substring(2, 2), 16), Convert.ToInt32(tdsVersion.Substring(4, 4), 16));

            byte serverNameLength = (byte)source.ReadByte();

            byte[] serverNameBytes = new byte[serverNameLength * 2];

            source.Read(serverNameBytes, 0, serverNameBytes.Length);

            ServerName = Encoding.Unicode.GetString(serverNameBytes);

            ServerVersion = new Version(source.ReadByte(), source.ReadByte(), (source.ReadByte() << 8) + source.ReadByte());

            return true;
        }

        /// <summary>
        /// Deflate the token
        /// </summary>
        /// <param name="destination">Stream to deflate token to</param>
        public override void Pack(MemoryStream destination)
        {
            destination.WriteByte((byte)TDSTokenType.LoginAcknowledgement);

            // Calculate the length of the token
            // The total length, in bytes, of the following fields: Interface, TDSVersion, Progname, and ProgVersion.
            ushort tokenLength = (ushort)(sizeof(byte) + sizeof(uint) + sizeof(byte) + (string.IsNullOrEmpty(ServerName) ? 0 : ServerName.Length * 2) + sizeof(uint));

            destination.WriteByte((byte)(tokenLength & 0xff));
            destination.WriteByte((byte)((tokenLength >> 8) & 0xff));

            destination.WriteByte((byte)Interface);

            uint tdsVersion = Convert.ToUInt32(string.Format("{0:X}", Math.Max(TDSVersion.Major, 0)) + string.Format("{0:X}", Math.Max(TDSVersion.Minor, 0)) + string.Format("{0:X2}", Math.Max(TDSVersion.Build, 0)) + string.Format("{0:X4}", Math.Max(TDSVersion.Revision, 0)), 16);

            destination.WriteByte((byte)((tdsVersion >> 24) & 0xff));
            destination.WriteByte((byte)((tdsVersion >> 16) & 0xff));
            destination.WriteByte((byte)((tdsVersion >> 8) & 0xff));
            destination.WriteByte((byte)(tdsVersion & 0xff));

            destination.WriteByte((byte)(string.IsNullOrEmpty(ServerName) ? 0 : ServerName.Length));

            byte[] serverNameBytes = Encoding.Unicode.GetBytes(ServerName);

            destination.Write(serverNameBytes, 0, serverNameBytes.Length);

            destination.WriteByte((byte)(ServerVersion.Major & 0xff));
            destination.WriteByte((byte)(ServerVersion.Minor & 0xff));
            destination.WriteByte((byte)((ServerVersion.Build >> 8) & 0xff));
            destination.WriteByte((byte)(ServerVersion.Build & 0xff));
        }

        /// <summary>
        /// TDS Token length
        /// </summary>
        /// <returns>Returns TDS Token length</returns>
        public override ushort Length() 
        {
            return 1;
        }

        public override bool Equals(object obj) 
        {
            return true;
        }


    }
}