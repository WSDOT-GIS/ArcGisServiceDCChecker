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

namespace ArcGisServiceDCChecker
{
	class Program
	{
		private static LicenseInitializer _aoLicenseInitializer = new ArcGisServiceDCChecker.LicenseInitializer();

		[STAThread()]
		static void Main(string[] args)
		{
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

			AGSServerConnection connection = null;
			IServerObjectManager4 som = null ;
			IServerContext ctxt = null;
			IMapServer mapServer = null;

			IEnumServerObjectConfigurationInfo socInfos;
			IServerObjectConfigurationInfo2 socInfo;
			try
			{
				// Open the output HTML file for writing.
				using (StreamWriter sw = new StreamWriter(Settings.Default.OutputFile))
				{
					sw.WriteLine("<!DOCTYPE html>");
					sw.WriteLine("<html>");
					sw.WriteLine("<head>");
					sw.WriteLine("<title>{0}</title>", "Results");
					// Add the stylesheet reference.
					sw.WriteLine("<link rel='stylesheet' type='text/css' href='{0}' />", Settings.Default.JQueryUICssUrl);
					sw.WriteLine("<link rel='stylesheet' type='text/css' href='{0}' />", "style.css");
					sw.WriteLine("</head>");
					sw.WriteLine("<body>");
					sw.WriteLine("<h1>ArcGIS Server Connection test results</h1>");
					sw.WriteLine("<p>This data can be used to determine which map services are still accessing data via non-direct-connect methods.</p>");
					// Loop through all of the hosts (servers) and create a div for each one containing map service test results.
					foreach (var host in servers)
					{
						sw.WriteLine("<div data-host='{0}'>", host);
						Console.Error.WriteLine("Server: {0}", host);
						sw.WriteLine("<h2>{0}</h2>", host);
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
									try
									{
										// Proceed only if the current service is a "MapServer".
										if (string.Compare(socInfo.TypeName, "MapServer", true) == 0)
										{
											// Create a div for the current map service.
											sw.WriteLine("<div data-map-server='{0}'>", socInfo.Name);
											sw.WriteLine("<h3 class='{0}'>{1}</h3>", socInfo.TypeName, socInfo.Name);
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
													sw.WriteLine("<dl><dt>{0}</dt><dd>{1}</dd></dl>", "Source Document", sourceDocName);
												}

												// Create a dictionary of the properties for all of the layers in the map service.
												List<string> propertyNames;
												var connectionProperties = mapServer.GetConnectionProperties(out propertyNames);
												// Write the properties to an HTML table.
												if (connectionProperties != null)
												{
													connectionProperties.WriteHtmlTable(propertyNames, sw);
												}
											}
											catch (COMException comEx)
											{
												// See if the exception was caused by a stopped service.  Just write the message in this case.
												if (comEx.ErrorCode == -2147467259)
												{
													sw.WriteLine("<p>{0}</p>", comEx.Message);
												}
												else
												{
													sw.WriteLine("<p>{0}: {1}</p>", comEx.ErrorCode, comEx.Message);
												}
											}
											sw.WriteLine("</div>"); // Close the div tag for the current map service.
										}
									}
									catch (Exception ex)
									{
										sw.WriteLine("<p class='error'>Exception: {0}</p>", ex.ToString());
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
								}
							}
						}
						catch (Exception ex)
						{
							sw.WriteLine("<p class='error'>Exception: {0}</p>", ex.ToString());
						}
						finally
						{
							connection.Dispose();
							sw.WriteLine("</div>"); // Close the div tag for the server.
							// Add JavaScript references for JQuery and the custom script for the results page.
						}
					}
					// Add the closing tags for the HTML document.
					sw.WriteLine("<script type='text/javascript' src='{0}'></script>", Settings.Default.JQueryUrl);
					sw.WriteLine("<script type='text/javascript' src='{0}'></script>", Settings.Default.JQueryUIUrl);
					sw.WriteLine("<script type='text/javascript' src='{0}'></script>", "results.js");
					sw.WriteLine("</body>");
					sw.WriteLine("</html>");
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




			//ESRI License Initializer generated code.
			//Do not make any call to ArcObjects after ShutDownApplication()
			_aoLicenseInitializer.ShutdownApplication();
		}
	}
}
