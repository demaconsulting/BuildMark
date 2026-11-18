<#
.SYNOPSIS
    Draws the README hero banner, docs/images/hero.png, from docs/images/hero/hero.svg.

.DESCRIPTION
    hero/hero.svg is the 1600x640 README banner; hero/dc-logo-dark.svg is the DEMA Consulting "DC" monogram it
    shows, redrawn as vector paths in colors for a dark background. Both are original to DEMA Consulting and follow
    the shared hero of CanvasNet (docs/images/hero). The banner is drawn with "dotnet canvasnet" from the tool
    manifest. Commit the changed hero.png with the change to the SVG that made it differ.

    Run it from anywhere; paths are taken from the repository the script is in.
#>
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Push-Location $repo
try {
    dotnet tool restore | Out-Null
    dotnet canvasnet render 'docs/images/hero/hero.svg' --strict --width 1600 -o 'docs/images/hero.png'
    if ($LASTEXITCODE -ne 0) {
        throw "canvasnet render failed with exit code $LASTEXITCODE"
    }
}
finally {
    Pop-Location
}
