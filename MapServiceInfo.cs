using System.Collections.Generic;

namespace ArcGisServiceDCChecker
{
	public class MapServiceInfo
	{
		public string MapServiceName { get; set; }
		public string SourceDocumentPath { get; set; }
		public List<ConnectionProperties> ConnectionProperties { get; set; }
		public string ErrorMessage { get; set; }
	}
}
