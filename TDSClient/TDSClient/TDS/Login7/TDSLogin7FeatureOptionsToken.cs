//  ---------------------------------------------------------------------------
//  <copyright file="TDSLogin7FeatureExtFedAuth.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Login7
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using TDSClient.TDS.Interfaces;
    using TDSClient.TDS.Utilities;

	/// <summary>
	/// Feature extension data delivered in the login packet
	/// </summary>
	public class TDSLogin7FeatureOptionsToken : List<TDSLogin7FeatureOptionToken>
	{
		/// <summary>
		/// Property used internally by inflation/deflation routine to tell caller how much data was read/written to the stream
		/// </summary>
		internal uint InflationSize { get; set; }

		//private readonly TDSDataClassification.Version dataClassificationVersion;

		/// <summary>
		/// Default constructor
		/// </summary>
		// public TDSLogin7FeatureOptionsToken(TDSDataClassification.Version dataClassificationVersion = TDSDataClassification.Version.V1)
		// {
		// 	this.dataClassificationVersion = dataClassificationVersion;
		// }

		/// <summary>
		/// Inflate an object instance from the stream
		/// </summary>
		public bool Unpack(MemoryStream source)
		{
			// Identifier of the feature
			TDSFeatureID featureID = TDSFeatureID.Terminator;

			// Iterate
			do
			{
				// Read the feature type
				featureID = (TDSFeatureID)source.ReadByte();

				// Token being inflated
				TDSLogin7FeatureOptionToken optionToken = null;

				// skip this feature extension
				switch (featureID)
				{
					case TDSFeatureID.FederatedAuthentication:
						{
							// Federated authentication
							optionToken = new TDSLogin7FedAuthOptionToken();
							break;
						}
					// case TDSFeatureID.SessionRecovery:
					// 	{
					// 		// Session recovery
					// 		optionToken = new TDSLogin7SessionRecoveryOptionToken();
					// 		break;
					// 	}
					// case TDSFeatureID.ClientTelemetry:
					// 	{
					// 		// Client telemetry
					// 		optionToken = new TDSLogin7ClientTelemetryOptionToken();
					// 		break;
					// 	}
					// case TDSFeatureID.DataClassification:
					// 	{
					// 		// Data classification
					// 		optionToken = new TDSLogin7DataClassificationOptionToken(dataClassificationVersion);
					// 		break;
					// 	}
					// case TDSFeatureID.SupportUTF8:
					// 	{
					// 		optionToken = new TDSLogin7ClientUTF8SupportOptionToken();
					// 		break;
					// 	}
					case TDSFeatureID.Terminator:
						{
							// Do nothing
							break;
						}
					default:
						{
							// Create a generic option
							optionToken = new TDSLogin7GenericOptionToken(featureID);
							break;
						}
				}

				// Check if we have an option token
				if (optionToken != null)
				{
					// Inflate it
					optionToken.Unpack(source);

					// Register with the collection
					Add(optionToken);

					// Update inflation offset
					InflationSize += optionToken.InflationSize;
				}
			}
			while (TDSFeatureID.Terminator != featureID);

			// We don't support continuation of inflation so report as fully inflated
			return true;
		}

		/// <summary>
		/// Serialize object into the stream
		/// </summary>
		/// <param name="destination"></param>
		public void Pack(MemoryStream destination)
		{
			// Deflate each feature extension
			foreach (TDSLogin7FeatureOptionToken option in this)
			{
				option.Pack(destination);
			}

			// Write the Terminator.
			destination.WriteByte((byte)TDSFeatureID.Terminator);
		}
	}
}