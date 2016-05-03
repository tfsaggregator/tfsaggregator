# get referenced DLL files from container
$containerList = "$env:TEMP\references.xml"
$flavor = '2013'
Invoke-RestMethod -Uri "$env:containerURL$flavor`?restype=container&comp=list" -OutFile $containerList
$files = Select-Xml -XPath "//Name" -Path $containerList | foreach { $_.Node.InnerText }
foreach ($file in $files) { Invoke-RestMethod -Uri "$env:containerURL$flavor/$file" -OutFile "$env:referenceFolder\$($flavor.Replace('-','.'))\$file" }
$flavor = '2015'
Invoke-RestMethod -Uri "$env:containerURL$flavor`?restype=container&comp=list" -OutFile $containerList
$files = Select-Xml -XPath "//Name" -Path $containerList | foreach { $_.Node.InnerText }
foreach ($file in $files) { Invoke-RestMethod -Uri "$env:containerURL$flavor/$file" -OutFile "$env:referenceFolder\$($flavor.Replace('-','.'))\$file" }
$flavor = '2015-1'
Invoke-RestMethod -Uri "$env:containerURL$flavor`?restype=container&comp=list" -OutFile $containerList
$files = Select-Xml -XPath "//Name" -Path $containerList | foreach { $_.Node.InnerText }
foreach ($file in $files) { Invoke-RestMethod -Uri "$env:containerURL$flavor/$file" -OutFile "$env:referenceFolder\$($flavor.Replace('-','.'))\$file" }
$flavor = '2015-2'
Invoke-RestMethod -Uri "$env:containerURL$flavor`?restype=container&comp=list" -OutFile $containerList
$files = Select-Xml -XPath "//Name" -Path $containerList | foreach { $_.Node.InnerText }
foreach ($file in $files) { Invoke-RestMethod -Uri "$env:containerURL$flavor/$file" -OutFile "$env:referenceFolder\$($flavor.Replace('-','.'))\$file" }
