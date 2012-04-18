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
using System.Linq;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;

namespace ArcGisServiceDCChecker
{
	public static class Extensions
	{
		/// <summary>
		/// Retrieves a list of connection properties from the layers of a <see cref="IMapServer"/>.
		/// </summary>
		/// <param name="mapServer">A map server.</param>
		/// <param name="propertyNames">The names of the keys in each dictionary of the output object.</param>
		/// <returns>
		/// A list of <see cref="KeyValuePair&lt;TKey,KValue&gt;"/> objects.  
		/// Each <see cref="KeyValuePair&lt;TKey,KValue&gt;.Key"/> corresponds to a data source name.
		/// Each value is a <see cref="Dictionary&lt;TKey,TValue&gt;"/> of connection properties.
		/// </returns>
		public static List<ConnectionProperties> GetConnectionProperties(this IMapServer mapServer /*, out List<string> propertyNames*/ )
		{
			IMapLayerInfos mapLayerInfos = null;
			IMapLayerInfo mapLayerInfo = null;
			IMapServerDataAccess mapServerDA = null;
			IMapServerInfo mapServerInfo = null; ;
			////propertyNames = new List<string>();
			IDataset dataset = null;
			IPropertySet connectionPropertySet = null;
			object namesObj, valuesObj;
			string[] names;
			object[] values;

			var output = new List<ConnectionProperties>();

			try
			{
				mapServerDA = (IMapServerDataAccess)mapServer;
				// Get the server info for the default map. (This application will assume that there is only a single map: the default.)
				string mapName = mapServer.DefaultMapName;
				mapServerInfo = mapServer.GetServerInfo(mapName);
				// Loop through all of the layers in the map service...
				mapLayerInfos = mapServerInfo.MapLayerInfos;

				ConnectionProperties connectionProperties;

				for (int i = 0, l = mapLayerInfos.Count; i < l; i++)
				{
					mapLayerInfo = mapLayerInfos.get_Element(i);
					if (mapLayerInfo.IsComposite)
					{
						continue;
					}
					
					connectionProperties = new ConnectionProperties(mapLayerInfo.Name);
					try
					{
						dataset = mapServerDA.GetDataSource(mapName, i) as IDataset;
					}
					catch (NotImplementedException ex)
					{
						connectionProperties["error"] = ex.Message;
						output.Add(connectionProperties);
						continue;
					}

					if (dataset != null)
					{
						connectionPropertySet = dataset.Workspace.ConnectionProperties;
						connectionPropertySet.GetAllProperties(out namesObj, out valuesObj);
						names = namesObj as string[];
						values = valuesObj as object[];

						string name;
						for (int j = 0; j < names.Length; j++)
						{
							name = names[j];
							connectionProperties[name] = values[j];
						}
						output.Add(connectionProperties);
					}
				}
			}
			finally
			{
				foreach (var item in new object[] {mapLayerInfos, mapLayerInfo, mapServerDA, mapServerInfo, dataset, connectionPropertySet})
				{
					if (item != null)
					{
						Marshal.ReleaseComObject(item);
					}
				}
			}

			return output;
		}

		/// <summary>
		/// Flattens a dictionary containing lists of <see cref="MapServiceInfo"/> objects into a list of dictionaries.  This format is more suitable for 
		/// conversion to a single table.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="propertyNames"></param>
		/// <returns></returns>
		public static List<Dictionary<string, object>> Flatten(this IDictionary<string, List<MapServiceInfo>> input, out HashSet<string> propertyNames)
		{
			const string
				serverNameName = "Server Name",
				mapServiceNameName = "Map Service Name",
				sourceDocumentPathName = "Source Document Path",
				featureClassName = "Feature Class",
				errorMessageName = "Error Message";

			var output = new List<Dictionary<string, object>>();

			propertyNames = new HashSet<string>();
			propertyNames.Add(serverNameName);
			propertyNames.Add(mapServiceNameName);
			propertyNames.Add(sourceDocumentPathName);
			propertyNames.Add(featureClassName);
			propertyNames.Add(errorMessageName);

			foreach (var serverItem in input)
			{
				foreach (var mapServiceInfo in serverItem.Value)
				{
					if (mapServiceInfo.ConnectionProperties == null)
					{
						var dict = new Dictionary<string, object>();

						dict[serverNameName] = serverItem.Key;
						dict[mapServiceNameName] = mapServiceInfo.MapServiceName;
						dict[sourceDocumentPathName] = mapServiceInfo.SourceDocumentPath;
						dict[errorMessageName] = mapServiceInfo.ErrorMessage;
					}
					else if (mapServiceInfo.ConnectionProperties.Count < 1)
					{
						var dict = new Dictionary<string, object>();
						dict[serverNameName] = serverItem.Key;
						dict[mapServiceNameName] = mapServiceInfo.MapServiceName;
						dict[sourceDocumentPathName] = mapServiceInfo.SourceDocumentPath;
						dict[errorMessageName] = mapServiceInfo.ErrorMessage;
						output.Add(dict);
					}
					else
					{
						foreach (var cp in mapServiceInfo.ConnectionProperties)
						{
							var dict = new Dictionary<string, object>();
							dict[serverNameName] = serverItem.Key;
							dict[mapServiceNameName] = mapServiceInfo.MapServiceName;
							dict[sourceDocumentPathName] = mapServiceInfo.SourceDocumentPath;
							dict[errorMessageName] = mapServiceInfo.ErrorMessage;
							dict[featureClassName] = cp.FeatureClassName;
							foreach (var kvp in cp.Properties)
							{
								propertyNames.Add(kvp.Key);
								dict[kvp.Key] = kvp.Value;
							}
							output.Add(dict);
						}
					}

				}
			}

			return output;
		}

		public static void ToCsv(this IDictionary<string, List<MapServiceInfo>> input, TextWriter writer)
		{
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}
			HashSet<string> propertyNamesHashSet;
			List<Dictionary<string, object>> flattened = input.Flatten(out propertyNamesHashSet);
			var propertyNames = propertyNamesHashSet.ToArray();

			// Create the column headings...
			for (int i = 0, l = propertyNames.Length; i < l; i++)
			{
				// Write the separator if this is not the first property name.
				if (i > 0) {
					writer.Write(',');
				}
				// Write the property name.
				writer.Write("\"{0}\"", propertyNames.ElementAt(i));

				// Write the new line character if this is the last property name.
				if (i == l - 1)
				{
					writer.WriteLine();
				}
			}

			// Add the data rows.
			foreach (var dict in flattened)
			{
				string propertyName;
				object propertyValue;
				for (int i = 0, l = propertyNames.Length; i < l; i++)
				{
					propertyName = propertyNames[i];
					if (i > 0)
					{
						writer.Write(',');
					}
					if (dict.ContainsKey(propertyName))
					{
						propertyValue = dict[propertyName];
						if (propertyValue != null)
						{
							if (propertyValue.GetType() == typeof(string))
							{
								writer.Write("\"{0}\"", propertyValue);
							}
							else
							{
								writer.Write(propertyValue);

							}
						}
						else
						{
							writer.Write(propertyValue);
						}
					}
				}
				writer.WriteLine();
			}
		}
	}
}
