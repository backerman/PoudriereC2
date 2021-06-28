# Deploy the database.

[CmdletBinding()]
param (
    # Also insert sample data.
    [Parameter()]
    [switch]
    $SampleData
)

$baseDir = Split-Path $PSCommandPath

Get-ChildItem (Join-Path $baseDir "createdb") |
    Where-Object -Property Name -Like "*.sql" |
    ForEach-Object { psql -f $_.FullName }

Get-ChildItem $baseDir |
    Where-Object -Property Name -Like "*.sql" |
    ForEach-Object { psql -f $_.FullName -d poudrierec2 }

if ($SampleData) {
    Get-ChildItem (Join-Path $baseDir "sample") |
        Where-Object -Property Name -Like "*.sql" |
	ForEach-Object { psql -f $_.FullName -d poudrierec2 }
}
