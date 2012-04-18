ArcGIS Service Non-Direct-Connect Checker
=========================================
This program checks all of the map services on specific ArcGIS Servers and reports the connection properties of the data used by those services.

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

Created by Jeff Jacobson
2011-08-25

## Configuration ##
Edit the .config file before running the .exe file.  The settings are described below.

### Servers ###
This is a comma-separated string containing the names of the servers that will be tested.

### OutputCsv ###
This setting controls where the output CSV file is written to.

### OutputJson ###
This setting controls where the output JSON file is written to.

## Limitations ##
* In order to run the application, you must be an administrator on all of the servers specified in the 'Servers' setting.
* Map services that are stopped will not be checked.
* This program is only compatible with version 10 and higher ArcGIS Servers.

## Notes ##
* If there are any problems accessing the data of a layer, it will be noted in the "Error" column of the row corresponding to that layer.