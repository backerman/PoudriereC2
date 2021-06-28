# Deploy the database.

[CmdletBinding()]
param (
    # Host to connect to.
    [Parameter()]
    [string]
    $PsqlHost,

    # User to connect as.
    [Parameter()]
    [string]
    $PsqlUser = "postgres",
    
    # Also insert sample data.
    [Parameter()]
    [switch]
    $SampleData
)

$baseDir = Split-Path $PSCommandPath

$psqlArgs = @()

if (-not [string]::IsNullOrWhiteSpace($PsqlHost)) {
    $psqlArgs += "-h", $PsqlHost
}

if (-not [string]::IsNullOrWhiteSpace($PsqlUser)) {
    $psqlArgs += "-U", $PsqlUser
}

Get-ChildItem (Join-Path $baseDir "createdb") |
    Where-Object -Property Name -Like "*.sql" |
    ForEach-Object {
        psql @psqlArgs -f $_.FullName
    }

# Commands from here use the database created above.
$psqlArgs += "-d", "poudrierec2"

Get-ChildItem $baseDir |
    Where-Object -Property Name -Like "*.sql" |
    ForEach-Object {
        psql @psqlArgs -f $_.FullName
    }

if ($SampleData) {
    Get-ChildItem (Join-Path $baseDir "sample") |
        Where-Object -Property Name -Like "*.sql" |
        ForEach-Object {
            psql @psqlArgs -f $_.FullName
        }
}
