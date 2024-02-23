//  ---------------------------------------------------------------------------
//  <copyright file="TDSFedAuthInfoOptionSTSURL.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.FedAuthInfo
{
    using System.IO;
    using System.Text;

    /// <summary>
    /// TDS FedAuth Info Option for STS URL
    /// </summary>
    public class TDSFedAuthInfoOptionSTSURL : TDSFedAuthInfoOption
    {
        /// <summary>
        /// STS URL
        /// </summary>
        public byte[] StsUrl;

        /// <summary>
        /// Return the FedAuthInfo Id.
        /// </summary>
        public override TDSFedAuthInfoId FedAuthInfoId
        {
            get
            {
                return TDSFedAuthInfoId.STSURL;
            }
        }

        /// <summary>
        /// Return the STSURL as a unicode string.
        /// </summary>
        public string STSURL
        {
            get
            {
                return StsUrl != null ? Encoding.Unicode.GetString(StsUrl) : null;
            }
        }

        /// <summary>
        /// Default public contructor
        /// </summary>
        public TDSFedAuthInfoOptionSTSURL()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="infoDataLength">Info Data Length</param>
        public TDSFedAuthInfoOptionSTSURL(uint infoDataLength) : this()
        {
            InfoDataLength = infoDataLength;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stsurl">STSURL string</param>
        public TDSFedAuthInfoOptionSTSURL(string stsurl)
            : this()
        {
            StsUrl = Encoding.Unicode.GetBytes(stsurl);
            InfoDataLength = (uint)StsUrl.Length;
        }

        /// <summary>
        /// Unpack the data from the stream, when receiving this token.
        /// </summary>
        public override bool Unpack(MemoryStream source)
        {
            if (InfoDataLength > 0)
            {
                StsUrl = new byte[InfoDataLength];
                source.Read(StsUrl, 0, StsUrl.Length);
            }

            return true;
        }

        /// <summary>
        /// Pack the data to the stream, when writing this token.
        /// </summary>
        /// <param name="source"></param>
        public override void Pack(MemoryStream source)
        {
            if (InfoDataLength > 0)
            {
                source.Write(StsUrl, 0, StsUrl.Length);
            }
        }
    }
}
