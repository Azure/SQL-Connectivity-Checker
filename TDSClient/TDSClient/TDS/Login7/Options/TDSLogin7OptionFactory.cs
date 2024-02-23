//  ---------------------------------------------------------------------------
//  <copyright file="TDSLogin7OptionFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//  </copyright>
//  ---------------------------------------------------------------------------

namespace TDSClient.TDS.Login7.Options
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using TDSClient.TDS.Utilities;

    /// <summary>
    /// TDS Login7 Packet Option Factory
    /// </summary>
    public class TDSLogin7OptionFactory
    {
        /// <summary>
        /// Option order within the packet
        /// </summary>
        private static readonly string[] OptionOrder = 
        { 
            "HostName",
            "UserName",
            "Password",
            "AppName",
            "ServerName",
            "Extension",
            "CltIntName",
            "Language",
            "Database",
            "ClientID",
            "SSPI",
            "AtchDBFile",
            "ChangePassword"
        };

        /// <summary>
        /// Text option types
        /// </summary>
        private static readonly string[] TextOptions =
        {
            "HostName",
            "UserName",
            "AppName",
            "ServerName",
            "CltIntName",
            "Language",
            "Database"
        };

        /// <summary>
        /// Password option types
        /// </summary>
        private static readonly string[] PasswordOptions =
        {
            "Password",
            "ChangePassword"
        };

        /// <summary>
        /// Creates a TDS Login7 packet option
        /// </summary>
        /// <param name="optionName">Option name</param>
        /// <param name="optionData">Option data</param>
        /// <returns>Created TDS Login7 packet option</returns>
        public static TDSLogin7Option CreateOption(string optionName, object optionData)
        {
            if (TextOptions.Contains(optionName))
            {
                if (optionData is string)
                {
                    var data = (string)optionData;
                    return new TDSLogin7TextOption(optionName, 0, Convert.ToUInt16(data.Length), data);
                } 
                else
                {
                    throw new InvalidOperationException();
                }
            }
            else if (PasswordOptions.Contains(optionName)) 
            {
                var data = (string)optionData;
                return new TDSLogin7PasswordOption(optionName, 0, Convert.ToUInt16(data.Length), data);
            } 
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Reads TDS Login7 options from a stream
        /// </summary>
        /// <param name="stream">Stream containing the Login7 packet</param>
        /// <returns>ClientID, TDS Login7 options pair</returns>
        public static Tuple<List<TDSLogin7Option>, byte[]> ReadOptionsFromStream(MemoryStream stream)
        {
            var options = new List<TDSLogin7Option>();
            var clientID = new byte[6];

            foreach (var option in OptionOrder)
            {
                ushort position = 0;
                ushort length = 0;

                switch (option)
                {
                    case "HostName":
                    case "UserName":
                    case "AppName":
                    case "ServerName":
                    case "CltIntName":
                    case "Language":
                    case "Database":
                        {
                            position = LittleEndianUtilities.ReadUShort(stream);
                            length = LittleEndianUtilities.ReadUShort(stream);

                            if (length != 0)
                            {
                                options.Add(new TDSLogin7TextOption(option, position, length));
                            }

                            break;
                        }

                    case "ClientID":
                        {
                            stream.Read(clientID, 0, 6);
                            break;
                        }

                    case "Password":
                    case "ChangePassword":
                        {
                            position = LittleEndianUtilities.ReadUShort(stream);
                            length = LittleEndianUtilities.ReadUShort(stream);

                            if (length != 0)
                            {
                                options.Add(new TDSLogin7PasswordOption(option, position, length));
                            }

                            break;
                        }

                    default:
                        {
                            position = LittleEndianUtilities.ReadUShort(stream);
                            length = LittleEndianUtilities.ReadUShort(stream);

                            if (length != 0)
                            {
                                throw new NotSupportedException();
                            }

                            break;
                        }
                }
            }

            // Ignore LongSSPI, not supported
            LittleEndianUtilities.ReadUInt(stream);

            options = options.OrderBy(opt => opt.Position).ToList();

            foreach (var option in options)
            {
                option.Unpack(stream);
            }

            return new Tuple<List<TDSLogin7Option>, byte[]>(options, clientID);
        }

        /// <summary>
        /// Writes TDS Login7 options to a stream
        /// </summary>
        /// <param name="stream">MemoryStream to write to</param>
        /// <param name="options">Options to write to the stream</param>
        /// <param name="clientID">ClientID to write to the stream</param>
        public static ushort WriteOptionsToStream(MemoryStream stream, List<TDSLogin7Option> options, byte[] clientID)
        {
            ushort currentPos = 94;
            
            foreach (var option in OptionOrder)
            {
                if (option != "ClientID")
                {
                    if(option == "Extension")
                    {
                        LittleEndianUtilities.WriteUShort(stream, currentPos);
                    }
                    else
                    {
                        LittleEndianUtilities.WriteUShort(stream, currentPos);

                    var tmp = options.Where(o => o.Name == option);
                    if (tmp.Any())
                    {
                        var opt = tmp.First();

                        LittleEndianUtilities.WriteUShort(stream, opt.Length);
                        opt.Position = currentPos;
                        currentPos += opt.TrueLength;
                    } 
                    else 
                    {
                        LittleEndianUtilities.WriteUShort(stream, 0);
                    }
                    }
                    
                }
                else
                {
                    stream.Write(clientID, 0, 6);
                }
            }

            // Ignore LongSSPI, not supported
            LittleEndianUtilities.WriteUInt(stream, 0);

            options = options.OrderBy(opt => opt.Position).ToList();

            foreach (var option in options)
            {
                option.Pack(stream);
            }

            return currentPos;
        }
    }
}
