//  ---------------------------------------------------------------------------
//  <copyright file="TDSLogin7FeatureExtFedAuth.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Login7
{
    using System.IO;
    using System.Collections.Generic;

	/// <summary>
	/// Feature extension data delivered in the login packet
	/// </summary>
	public class TDSLogin7FeatureOptionsToken : List<TDSLogin7FeatureOptionToken>
	{
		/// <summary>
		/// Property used internally by unpack/ routine to tell caller how much data was read/written to the stream
		/// </summary>
		internal uint Size { get; set; }

		//private readonly TDSDataClassification.Version dataClassificationVersion;

		/// <summary>
		/// Default constructor
		/// </summary>
		// public TDSLogin7FeatureOptionsToken(TDSDataClassification.Version dataClassificationVersion = TDSDataClassification.Version.V1)
		// {
		// 	this.dataClassificationVersion = dataClassificationVersion;
		// }

		/// <summary>
		/// Unpack an object instance from the stream
		/// </summary>
		/// 
		public bool Unpack(MemoryStream source)
		{
			TDSFeatureID featureID = TDSFeatureID.Terminator;

			do
			{
				featureID = (TDSFeatureID)source.ReadByte();

				TDSLogin7FeatureOptionToken optionToken = null;

				switch (featureID)
				{
					case TDSFeatureID.FederatedAuthentication:
						{
							optionToken = new TDSLogin7FedAuthOptionToken();
							break;
						}
					case TDSFeatureID.Terminator:
						{
							break;
						}
					default:
						{
							optionToken = new TDSLogin7GenericOptionToken(featureID);
							break;
						}
				}

				if (optionToken != null)
				{
					optionToken.Unpack(source);

					Add(optionToken);

					Size += optionToken.Size;
				}
			}
			while (TDSFeatureID.Terminator != featureID);

			return true;
		}

		/// <summary>
		/// Pack object into the stream
		/// </summary>
		/// <param name="destination"></param>
		public void Pack(MemoryStream destination)
		{
			foreach (TDSLogin7FeatureOptionToken option in this)
			{
				option.Pack(destination);
			}

			destination.WriteByte((byte)TDSFeatureID.Terminator);
		}
	}
}