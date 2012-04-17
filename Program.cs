/*
ArcGIS Service Non-Direct-Connect Checker
Copyright (C) 2011 Washington State Department of Transportation

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using ArcGisServiceDCChecker.Properties;
using ESRI.ArcGIS.ADF.Connection.AGS;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Server;
using Newtonsoft.Json;
////using System.Web.Script.Serialization;

namespace ArcGisServiceDCChecker
{
	class Program
	{
		private static LicenseInitializer _aoLicenseInitializer = new ArcGisServiceDCChecker.LicenseInitializer();

		[STAThread()]
		static void Main(string[] args)
		{
			Dictionary<string, List<MapServiceInfo>> output;
			////JavaScriptSerializer serializer;

			var serializerSettings = new JsonSerializerSettings
			{
				Formatting = Formatting.Indented,
			};
#if DEBUG
			if (File.Exists("MapServers.json"))
			{
				////serializer = new JavaScriptSerializer();
				////output = serializer.Deserialize<Dictionary<string, List<MapServiceInfo>>>(File.ReadAllText("MapServers.json"));

				output = JsonConvert.DeserializeObject<Dictionary<string, List<MapServiceInfo>>>(File.ReadAllText("MapServers.json"));

				// Write to CSV...
				using (StreamWriter writer = new StreamWriter("MapServers.csv"))
				{
					output.ToCsv(writer);
				}
				return;
			}
#endif

			// Get the server names from the config.
			var servers = Settings.Default.Servers.Split(',');


			Console.Error.WriteLine("Getting license...");
			//ESRI License Initializer generated code.
			if (!_aoLicenseInitializer.InitializeApplication(new esriLicenseProductCode[] { esriLicenseProductCode.esriLicenseProductCodeArcView },
			new esriLicenseExtensionCode[] { }))
			{
				Console.Error.WriteLine(_aoLicenseInitializer.LicenseMessage());
				Console.Error.WriteLine("This application could not initialize with the correct ArcGIS license and will shutdown.");
				_aoLicenseInitializer.ShutdownApplication();
				return;
			}
			Console.Error.WriteLine("License acquired.");

			output = CollectMapServerInfo(servers);

			////serializer = new JavaScriptSerializer();
			////string json = serializer.Serialize(output);
			////File.WriteAllText("MapServers.json", json);

			string json = JsonConvert.SerializeObject(output, serializerSettings);
			File.WriteAllText("MapServers.json", json);


			// Write to CSV...
			using (StreamWriter writer = new StreamWriter("MapServers.csv"))
			{
				output.ToCsv(writer);
			}


			//////ESRI License Initializer generated code.
			//////Do not make any call to ArcObjects after ShutDownApplication()
			////_aoLicenseInitializer.ShutdownApplication();
		}

		private static Dictionary<string, List<MapServiceInfo>> CollectMapServerInfo(IEnumerable<string> servers)
		{
			Dictionary<string, List<MapServiceInfo>> output;
			output = new Dictionary<string, List<MapServiceInfo>>();
			AGSServerConnection connection = null;
			IServerObjectManager4 som = null;
			IServerContext ctxt = null;
			IMapServer mapServer = null;

			IEnumServerObjectConfigurationInfo socInfos;
			IServerObjectConfigurationInfo2 socInfo;
			MapServiceInfo mapServiceInfo;
			try
			{
				// Loop through all of the hosts (servers) and create a div for each one containing map service test results.
				foreach (var host in servers)
				{
					var mapServiceInfos = new List<MapServiceInfo>();
					output.Add(host, mapServiceInfos);
					// Create the connection object
					connection = new AGSServerConnection(host, null, false, true);
					try
					{
						// Attempt to connect to the server.
						connection.Connect(false);
						if (connection.IsConnected)
						{
							// Get the Server Object Manager (SOM) for the current ArcGIS Server.
							som = (IServerObjectManager4)connection.ServerObjectManager;
							// Get a list of the services on the server.
							socInfos = som.GetConfigurationInfos();
							// Get the first service from the list.
							socInfo = socInfos.Next() as IServerObjectConfigurationInfo2;

							// Loop through the list of services...
							while (socInfo != null)
							{
								mapServiceInfo = new MapServiceInfo { MapServiceName = socInfo.Name };
								try
								{
									// Proceed only if the current service is a "MapServer".
									if (string.Compare(socInfo.TypeName, "MapServer", true) == 0)
									{
										// Create a div for the current map service.
										//sw.WriteLine("<div data-map-server='{0}'>", socInfo.Name);
										//sw.WriteLine("<h3 class='{0}'>{1}</h3>", socInfo.TypeName, socInfo.Name);
										Console.Error.WriteLine("{0}", socInfo.Name);
										try
										{
											// Create a server context for the current map service.
											ctxt = som.CreateServerContext(socInfo.Name, socInfo.TypeName);
											// Cast the context object to an IMapServer.
											mapServer = (IMapServer)ctxt.ServerObject;

											// Get the document name
											IMapServerInit2 msInit = null;
											msInit = mapServer as IMapServerInit2;
											string sourceDocName = msInit != null ? msInit.FilePath : null;
											if (sourceDocName != null)
											{
												mapServiceInfo.SourceDocumentPath = sourceDocName;
											}

											// Create a dictionary of the properties for all of the layers in the map service.
											mapServiceInfo.ConnectionProperties = mapServer.GetConnectionProperties();
										}
										catch (COMException comEx)
										{
											// See if the exception was caused by a stopped service.  Just write the message in this case.
											if (comEx.ErrorCode == -2147467259)
											{
												mapServiceInfo.ErrorMessage = comEx.Message;
											}
											else
											{
												mapServiceInfo.ErrorMessage = string.Format("{0}: {1}", comEx.ErrorCode, comEx.Message);
											}
										}
									}
								}
								catch (Exception ex)
								{
									mapServiceInfo.ErrorMessage = ex.ToString();
									////sw.WriteLine("<p class='error'>Exception: {0}</p>", ex.ToString());
								}
								finally
								{
									// Release the server context.
									if (ctxt != null)
									{
										ctxt.ReleaseContext();
									}

									// Go to the next soc info.
									socInfo = socInfos.Next() as IServerObjectConfigurationInfo2;
								}
								mapServiceInfos.Add(mapServiceInfo);
							}
						}
					}
					////catch (Exception ex)
					////{
					////    sw.WriteLine("<p class='error'>Exception: {0}</p>", ex.ToString());
					////}
					finally
					{
						connection.Dispose();
					}
				}
			}
			finally
			{
				// Release any COM objects that have been created.
				if (mapServer != null)
				{
					Marshal.ReleaseComObject(mapServer);
				}
				if (ctxt != null)
				{
					Marshal.ReleaseComObject(ctxt);
				}
				if (som != null)
				{
					Marshal.ReleaseComObject(som);
				}
			}
			return output;
		}
	}
}
