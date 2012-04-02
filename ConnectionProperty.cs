using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace ArcGisServiceDCChecker
{
	public class ConnectionProperties
	{
		public string FeatureClassName { get; set; }
		public Dictionary<string, object> Properties { get; protected set; }

		public object this[string index]
		{
			get { return this.Properties[index]; }
			set {
				// If value is a byte[], convert to base64 string before assigning to dictionary.
				byte[] bytes = value as byte[];
				if (bytes != null)
				{
					value = Convert.ToBase64String(bytes, Base64FormattingOptions.None);
				}
				this.Properties[index] = value; 
			}
		}

		public ConnectionProperties()
		{
			this.Properties = new Dictionary<string, object>();
		}

		public ConnectionProperties(string featureClassName) : this()
		{
			this.FeatureClassName = featureClassName;
		}

	}
}
