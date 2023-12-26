//  ---------------------------------------------------------------------------
//  <copyright file="TDSFedAuthInfoOptionSPN.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.FedAuthInfo
{
    using System.IO;
    using System.Text;

    /// <summary>
    /// TDS FedAuth Info Option for SPN
    /// </summary>
    public class TDSFedAuthInfoOptionSPN : TDSFedAuthInfoOption
    {
        /// <summary>
        /// Information Data Length
        /// </summary>
        public uint InfoDataLength;

        /// <summary>
        /// STS URL
        /// </summary>
        public byte[] SPN;

        /// <summary>
        /// Return the SPN as a unicode string.
        /// </summary>
        public string GetSPNString
        {
            get
            {
                if (SPN != null)
                {
                    return Encoding.Unicode.GetString(SPN);
                }

                return null;
            }
        }

        /// <summary>
        /// Return the FedAuthInfo Id.
        /// </summary>
        public override TDSFedAuthInfoId FedAuthInfoId
        {
            get
            {
                return TDSFedAuthInfoId.SPN;
            }
        }

        /// <summary>
        /// Default public contructor
        /// </summary>
        public TDSFedAuthInfoOptionSPN()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="infoDataLength">Info Data Length</param>
        public TDSFedAuthInfoOptionSPN(uint infoDataLength)
            : this()
        {
            InfoDataLength = infoDataLength;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="spn">SPN string</param>
        public TDSFedAuthInfoOptionSPN(string spn)
            : this()
        {
            SPN = Encoding.Unicode.GetBytes(spn);
            InfoDataLength = (uint)SPN.Length;
        }

        /// <summary>
        /// Inflate the data from the stream, when receiving this token.
        /// </summary>
        public override bool Unpack(MemoryStream source)
        {
            // Read the information data
            // 
            if (InfoDataLength > 0)
            {
                SPN = new byte[InfoDataLength];
                source.Read(SPN, 0, SPN.Length);
            }

            return true;
        }

        /// <summary>
        /// Deflate the data to the stream, when writing this token.
        /// </summary>
        /// <param name="source"></param>
        public override void Pack(MemoryStream source)
        {
            if (InfoDataLength > 0)
            {
                source.Write(SPN, 0, SPN.Length);
            }
        }
    }
}
