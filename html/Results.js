/*global jQuery */
(function($) {
	"use strict";
	
	function processData(data) {
		var serverName, server, currentSection, mapService, mapServiceSection, i, il, servicePropertyList, connectionProperties, table;
		
		console.debug(data);
		
		function getConnectionPropertyNames(/*{Array}*/ connectionProperties) {
			var i, l, props, name, names = {}, output = [];
			
			if (typeof(connectionProperties) !== "undefined") {
				
				// Add each 
				for (i = 0, l = connectionProperties.length; i < l; i += 1) {
					props = connectionProperties[i].Properties;
					// Add a property with the name to the names array.  We don't care what the value is, so set it to null.
					for (name in props) {
						if (props.hasOwnProperty(name)) {
							names[name] = null;
						}
					}
				}
				
				// Loop through names object's properties add return them as an array.
				for (name in names) {
					if (names.hasOwnProperty(name)) {
						output.push(name);
					}
				}
			}
			
			return output;
		}
		
		function processConnectionProperties(/*{Array}*/ connectionProperties) {
			var table, thead, tbody, tr, td, names = getConnectionPropertyNames(connectionProperties), i, l, n, nl, props;
			
			
			table = $("<table>");
			
			thead = $("<thead>").appendTo(table);
			tr = $("<tr>").appendTo(thead);
			
			// Add feature class name header
			$("<th>FC Name</th>").appendTo(tr);
			// Loop through all of the property names and create column headers for each.
			for (n = 0, nl = names.length; n < nl; n += 1) {
				$("<th>").text(names[n]).appendTo(tr);
			}
			
			tbody = $("<tbody>").appendTo(table);
			
			for (i = 0, l = connectionProperties.length; i < l; i += 1) {
				tr = $("<tr>").appendTo(tbody);
				props = connectionProperties[i].Properties;
				// Add feature class name.
				$("<td>").appendTo(tr).text(connectionProperties[i].FeatureClassName);
				for (n = 0, nl = names.length; n < nl; n += 1) {
					td = $("<td>").appendTo(tr);
					td.text(props[names[n]]);
				}
			}
			
			return table;
		}
		
		// Loop through the list of servers.  Each property of data is corresponds to a server name.
		for (serverName in data) {
			if (data.hasOwnProperty(serverName)) {
				server = data[serverName];

				currentSection = $("<section>").attr("id", serverName).appendTo("body");
				$("<h1>").text(serverName).appendTo(currentSection);
				
				// Loop through the list of map services.
				for (i = 0, il = server.length; i < il; i += 1) {
					mapService = server[i];
					mapServiceSection = $("<section>").appendTo(currentSection);
					$("<h1>").text(mapService.MapServerName).appendTo(mapServiceSection);
					if (mapService.SourceDocumentPath) {
						$("<p>").text(mapService.SourceDocumentPath).appendTo(mapServiceSection);
					}
					if (mapService.ErrorMessage) {
						$("<p class='error'>").text(mapService.ErrorMessage).appendTo(mapServiceSection);
					}
					
					if (typeof(mapService.ConnectionProperties) !== "undefined" && mapService.ConnectionProperties !== null && mapService.ConnectionProperties.length) {
						table = processConnectionProperties(mapService.ConnectionProperties);
						
						table.appendTo(mapServiceSection);
					}
				}
			}
		}
	}
	
	$(document).ready(function(){
		$.getJSON('MapServers.json', processData);
	});
	
}(jQuery));
