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
using System.Text.RegularExpressions;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;

namespace ArcGisServiceDCChecker
{
	public static class Extensions
	{
		/// <summary>
		/// Writes the results of the <see cref="GetConnectionProperties(this IMapServer, out List<string>)"/> method to an HTML table.
		/// </summary>
		/// <param name="connectionProperties">The results of <see cref="GetConnectionProperties(this IMapServer, out List<string>)"/></param>
		/// <param name="propertyNames">The list of property names output from <see cref="GetConnectionProperties(this IMapServer, out List<string>)"/></param>
		/// <param name="writer">A <see cref="TextWriter"/> that the HTML will be written to.</param>
		public static void WriteHtmlTable(this IEnumerable<KeyValuePair<string, Dictionary<string, object>>> connectionProperties, List<string> propertyNames, TextWriter writer)
		{
			var nonDCRe = new Regex(@"^\d+$"); // This will match a purely numeric string;
			writer.WriteLine("<table>");
			writer.WriteLine("<thead><tr>");
			writer.WriteLine("<th>Layer</th>");
			for (int i = 0; i < propertyNames.Count; i++)
			{
				writer.Write("<th>{0}</th>", propertyNames[i]);
			}
			writer.WriteLine("</tr></thead>");
			writer.WriteLine("<tbody>");
			foreach (var layerKvp in connectionProperties)
			{
				writer.Write("<tr>");
				writer.Write("<td>{0}</td>", layerKvp.Key);
				Dictionary<string, object> props = layerKvp.Value;
				string propName;
				object value;
				for (int i = 0; i < propertyNames.Count; i++)
				{
					propName = propertyNames[i];
					if (props.Keys.Contains(propName))
					{
						value = props[propName];
						if (string.Compare(propName, "Instance", StringComparison.OrdinalIgnoreCase) == 0)
						{
							string cls = value != null && nonDCRe.IsMatch(value.ToString()) ? "nondc" : "dc";
							writer.Write("<td class='{1}'>{0}</td>", value, cls);
						}
						else
						{
							writer.Write("<td>{0}</td>", value);
						}

					}
					else
					{
						writer.Write("<td />");
					}
				}
				writer.Write("</tr>");
			}
			writer.WriteLine("</tbody>");
			writer.WriteLine("</table>");
		}

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
		public static List<KeyValuePair<string, Dictionary<string, object>>> GetConnectionProperties(this IMapServer mapServer, out List<string> propertyNames)
		{
			IMapLayerInfos mapLayerInfos = null;
			IMapLayerInfo mapLayerInfo = null;
			IMapServerDataAccess mapServerDA = null;
			IMapServerInfo mapServerInfo = null; ;
			propertyNames = new List<string>();
			IDataset dataset = null;
			IPropertySet connectionProperties = null;
			object namesObj, valuesObj;
			string[] names;
			object[] values;

			var output = new List<KeyValuePair<string, Dictionary<string, object>>>();

			try
			{
				mapServerDA = (IMapServerDataAccess)mapServer;
				// Get the server info for the default map. (This application will assume that there is only a single map: the default.)
				string mapName = mapServer.DefaultMapName;
				mapServerInfo = mapServer.GetServerInfo(mapName);
				// Loop through all of the layers in the map service...
				mapLayerInfos = mapServerInfo.MapLayerInfos;

				for (int i = 0, l = mapLayerInfos.Count; i < l; i++)
				{
					mapLayerInfo = mapLayerInfos.get_Element(i);
					if (mapLayerInfo.IsComposite)
					{
						continue;
					}
					
					var dict = new Dictionary<string, object>();
					try
					{
						dataset = mapServerDA.GetDataSource(mapName, i) as IDataset;
					}
					catch (NotImplementedException ex)
					{
						dict.Add("Error", ex.Message);
						output.Add(new KeyValuePair<string, Dictionary<string, object>>(mapLayerInfo.Name, dict));
						if (!propertyNames.Contains("Error"))
						{
							propertyNames.Add("Error");
						}
						continue;
					}

					if (dataset != null)
					{
						connectionProperties = dataset.Workspace.ConnectionProperties;
						connectionProperties.GetAllProperties(out namesObj, out valuesObj);
						names = namesObj as string[];
						values = valuesObj as object[];

						

						string name;
						for (int j = 0; j < names.Length; j++)
						{
							name = names[j];
							if (!propertyNames.Contains(name))
							{
								propertyNames.Add(name);
							}
							dict.Add(name, values[j]);
						}
						output.Add(new KeyValuePair<string, Dictionary<string, object>>(dataset.Name, dict));
					}
				}
			}
			finally
			{
				foreach (var item in new object[] {mapLayerInfos, mapLayerInfo, mapServerDA, mapServerInfo, dataset, connectionProperties})
				{
					if (item != null)
					{
						Marshal.ReleaseComObject(item);
					}
				}
			}

			return output;
		}
	}
}
