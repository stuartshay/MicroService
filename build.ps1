[CmdletBinding()]
Param(
    [string]$Script = "build.cake",
    [string]$BuildEnvironment = "local",
    [string]$Target = "Default",
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",
    [ValidateSet("Quiet", "Minimal", "Normal", "Verbose", "Diagnostic")]
    [string]$Verbosity = "Verbose",
    [Alias("DryRun","Noop")]
    [switch]$WhatIf,
    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string[]]$ScriptArgs
)

Write-Host -f Yellow "Preparing to run build script..."
if(!$PSScriptRoot){
    $PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
}

$UseDryRun = "";
if($WhatIf.IsPresent) 
{
    $UseDryRun = "--dryrun"
}

Write-Host -f Yellow "Looking for Cake.Tool..."
if (-Not (& dotnet tool list -g | Select-String "cake.tool")) {
    & dotnet tool install  --tool-path . Cake.Tool
}

Write-Host -f Yellow "Running build script..."
if($BuildEnvironment -eq "cloud") {
    &  .\dotnet-cake $Script --nuget_useinprocessclient=true --target=$Target --configuration=$Configuration --verbosity=$Verbosity $UseDryRun $ScriptArgs
}
else {
    &  dotnet cake $Script --nuget_useinprocessclient=true --target=$Target --configuration=$Configuration --verbosity=$Verbosity $UseDryRun $ScriptArgs
}
exit $LASTEXITCODE
