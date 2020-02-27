using System.IO;
using TDSClient.TDS.Interfaces;

namespace TDSClient.TDS.Login7
{
    public enum TDSLogin7OptionFlags3ChangePassword : byte
    {
        NoChangeRequest,
        RequestChange
    }

    public enum TDSLogin7OptionFlags3SendYukonBinaryXML : byte
    {
        Off,
        On
    }

    public enum TDSLogin7OptionFlags3UserInstanceProcess : byte
    {
        DontRequestSeparateProcess,
        RequestSeparateProcess
    }

    public enum TDSLogin7OptionFlags3UnknownCollationHandling : byte
    {
        Off,
        On
    }

    public enum TDSLogin7OptionFlags3Extension : byte
    {
        DoesntExist,
        Exists
    }

    public class TDSLogin7OptionFlags3 : IPackageable
    {
        /// <summary>
        ///  Specifies whether the login request SHOULD change password.
        /// </summary>
        public TDSLogin7OptionFlags3ChangePassword ChangePassword { get; set; }

        /// <summary>
        /// On if XML data type instances are returned as binary XML.
        /// </summary>
        public TDSLogin7OptionFlags3SendYukonBinaryXML SendYukonBinaryXML { get; set; }

        /// <summary>
        /// On if client is requesting separate process to be spawned as user instance.
        /// </summary>
        public TDSLogin7OptionFlags3UserInstanceProcess UserInstanceProcess { get; set; }

        /// <summary>
        /// This bit is used by the server to determine if a client is able to
        /// properly handle collations introduced after TDS 7.2. 
        /// </summary>
        public TDSLogin7OptionFlags3UnknownCollationHandling UnknownCollationHandling { get; set; }

        /// <summary>
        /// Specifies whether ibExtension/cbExtension fields are used.
        /// </summary>
        public TDSLogin7OptionFlags3Extension Extension { get; set; }

        public void Pack(MemoryStream stream)
        {
            byte packedByte = (byte)((byte)ChangePassword
                | ((byte)UserInstanceProcess << 1)
                | ((byte)SendYukonBinaryXML << 2)
                | ((byte)UnknownCollationHandling << 3)
                | ((byte)Extension << 4));

            stream.WriteByte(packedByte);
        }

        public bool Unpack(MemoryStream stream)
        {
            byte flagByte = (byte)stream.ReadByte();

            ChangePassword = (TDSLogin7OptionFlags3ChangePassword)(flagByte & 0x01);
            UserInstanceProcess = (TDSLogin7OptionFlags3UserInstanceProcess)((flagByte >> 1) & 0x01);
            SendYukonBinaryXML = (TDSLogin7OptionFlags3SendYukonBinaryXML)((flagByte >> 2) & 0x01);
            UnknownCollationHandling = (TDSLogin7OptionFlags3UnknownCollationHandling)((flagByte >> 3) & 0x01);
            Extension = (TDSLogin7OptionFlags3Extension)((flagByte >> 4) & 0x01);
            return true;
        }
    }
}
