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
        /// Service Principal Name (SPN)
        /// </summary>
        public byte[] ServicePrincipalName;

        /// <summary>
        /// Return the SPN as a unicode string.
        /// </summary>
        public string GetSPNString
        {
            get
            {
                return ServicePrincipalName != null ? Encoding.Unicode.GetString(ServicePrincipalName) : null;
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
            ServicePrincipalName = Encoding.Unicode.GetBytes(spn);
            InfoDataLength = (uint)ServicePrincipalName.Length;
        }

        /// <summary>
        /// Unpack the data from the stream, when receiving this token.
        /// </summary>
        public override bool Unpack(MemoryStream source)
        {
            if (InfoDataLength > 0)
            {
                ServicePrincipalName = new byte[InfoDataLength];
                source.Read(ServicePrincipalName, 0, ServicePrincipalName.Length);
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
                source.Write(ServicePrincipalName, 0, ServicePrincipalName.Length);
            }
        }
    }
}