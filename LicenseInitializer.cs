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
using ESRI.ArcGIS;

namespace ArcGisServiceDCChecker
{
	internal partial class LicenseInitializer
	{
		public LicenseInitializer()
		{
			ResolveBindingEvent += new EventHandler(BindingArcGISRuntime);
		}

		void BindingArcGISRuntime(object sender, EventArgs e)
		{
			//
			// TODO: Modify ArcGIS runtime binding code as needed
			//
			if (!RuntimeManager.Bind(ProductCode.Desktop))
			{
				// Failed to bind, announce and force exit
				Console.WriteLine("Invalid ArcGIS runtime binding. Application will shut down.");
				System.Environment.Exit(0);
			}
		}
	}
}