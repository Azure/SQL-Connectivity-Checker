using System;
using System.IO;
using System.Text;

using TDSClient.TDS.Login7;
using TDSClient.TDS.Utilities;
namespace TDSClient.TDS.Tokens
{
    /// <summary>
    /// Login acknowledgement token.
    /// </summary>
    #pragma warning disable CS0659
    public class TDSLoginAckToken : TDSToken
    #pragma warning restore CS0659
    {
        /// <summary>
        /// The type of interface with which the server will accept client requests
        /// </summary>
        public ushort TokenLength { get; set; }

        /// <summary>
        /// The type of interface with which the server will accept client requests
        /// </summary>
        public TDSLogin7TypeFlagsSQLType Interface { get; set; }

        /// <summary>
        /// TDS Version used by the server
        /// </summary>
        public Version TDSVersion { get; set; }

        /// <summary>
        /// Name of the server (e.g. "Microsoft SQL Server")
        /// </summary>
        public string ProgName { get; set; }

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
            ProgName = serverName;
        }

        /// <summary>
        /// Unpack the token
        /// NOTE: This operation is not continuable and assumes that the entire token is available in the stream
        /// </summary>
        /// <param name="source">Stream to inflate the token from</param>
        public override bool Unpack(MemoryStream source)
        {
            // We skip the token identifier because it is read by token factory
            
            TokenLength = (ushort)(source.ReadByte() + (source.ReadByte() << 8));

            Interface = (TDSLogin7TypeFlagsSQLType)source.ReadByte();

            string tdsVersion = string.Format("{0:X}", (uint)(source.ReadByte() << 24)
                + (uint)(source.ReadByte() << 16)
                + (uint)(source.ReadByte() << 8)
                + (uint)source.ReadByte());

            // Consturct TDS version
            // See tds.h for TDSXX clarifications
            TDSVersion = new Version(int.Parse(tdsVersion.Substring(0, 1)), int.Parse(tdsVersion.Substring(1, 1)), Convert.ToInt32(tdsVersion.Substring(2, 2), 16), Convert.ToInt32(tdsVersion.Substring(4, 4), 16));

            byte serverNameLength = (byte)source.ReadByte();

            byte[] serverNameBytes = new byte[serverNameLength * 2];

            source.Read(serverNameBytes, 0, serverNameBytes.Length);

            ProgName = Encoding.Unicode.GetString(serverNameBytes);

            ServerVersion = new Version(source.ReadByte(), source.ReadByte(), (source.ReadByte() << 8) + source.ReadByte());

            return true;
        }

        /// <summary>
        /// Deflate the token
        /// </summary>
        /// <param name="destination">Stream to deflate token to</param>
        public override void Pack(MemoryStream destination)
        {
            destination.WriteByte((byte)TDSTokenType.LoginAck);

            // Calculate the length of the token
            // The total length, in bytes, of the following fields: Interface, TDSVersion, Progname, and ProgVersion.
            ushort tokenLength = (ushort)(sizeof(byte) + sizeof(uint) + sizeof(byte) + (string.IsNullOrEmpty(ProgName) ? 0 : ProgName.Length * 2) + sizeof(uint));

            destination.WriteByte((byte)(tokenLength & 0xff));
            destination.WriteByte((byte)((tokenLength >> 8) & 0xff));

            destination.WriteByte((byte)Interface);

            uint tdsVersion = Convert.ToUInt32(string.Format("{0:X}", Math.Max(TDSVersion.Major, 0)) + string.Format("{0:X}", Math.Max(TDSVersion.Minor, 0)) + string.Format("{0:X2}", Math.Max(TDSVersion.Build, 0)) + string.Format("{0:X4}", Math.Max(TDSVersion.Revision, 0)), 16);

            destination.WriteByte((byte)((tdsVersion >> 24) & 0xff));
            destination.WriteByte((byte)((tdsVersion >> 16) & 0xff));
            destination.WriteByte((byte)((tdsVersion >> 8) & 0xff));
            destination.WriteByte((byte)(tdsVersion & 0xff));

            destination.WriteByte((byte)(string.IsNullOrEmpty(ProgName) ? 0 : ProgName.Length));

            byte[] serverNameBytes = Encoding.Unicode.GetBytes(ProgName);

            destination.Write(serverNameBytes, 0, serverNameBytes.Length);

            destination.WriteByte((byte)(ServerVersion.Major & 0xff));
            destination.WriteByte((byte)(ServerVersion.Minor & 0xff));
            destination.WriteByte((byte)((ServerVersion.Build >> 8) & 0xff));
            destination.WriteByte((byte)(ServerVersion.Build & 0xff));
        }

        /// <summary>
        /// LoginAck token length
        /// </summary>
        /// <returns>Returns LoginAck length</returns>
        public override ushort Length() 
        {
            return (ushort)(sizeof(byte) + sizeof(ushort) + TokenLength);
        }

        /// <summary>
		/// Compares.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as TDSLoginAckToken);
        }

		/// <summary>
		/// Compares.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
        public bool Equals(TDSLoginAckToken obj)
		{
			return Length() == obj.Length()
					&& TokenLength == obj.TokenLength
					&& Interface.Equals(obj.TokenLength)
                    && TDSVersion.Equals(obj.TDSVersion)
                    && ProgName.Equals(obj.ProgName);
		}

        /// <summary>
        /// Process token.
        /// </summary>
        public override void ProcessToken()
        {
            LoggingUtilities.WriteLog($"  Client received LoginAck token:");
            LoggingUtilities.WriteLog(ProgName);
            LoggingUtilities.WriteLog(ServerVersion.ToString());
            LoggingUtilities.WriteLog(TDSVersion.ToString());

            LoggingUtilities.WriteLog("Logged in successfully.");
        }
    }
}