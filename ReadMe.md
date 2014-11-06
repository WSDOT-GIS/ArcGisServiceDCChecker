ArcGIS Service Non-Direct-Connect Checker
=========================================
This program checks all of the map services on specific ArcGIS Servers and reports the connection properties of the data used by those services.

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
