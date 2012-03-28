using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGisServiceDCChecker
{
	public class MapServiceInfo
	{
		public string MapServerName { get; set; }
		public string SourceDocumentPath { get; set; }
		public Dictionary<string, Dictionary<string, object>> ConnectionProperties { get; set; }
		public string ErrorMessage { get; set; }
	}
}
