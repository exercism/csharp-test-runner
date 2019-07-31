<#
.SYNOPSIS
    Run a solution's tests.
.DESCRIPTION
    Run a solution's tests.
.PARAMETER Exercise
    The slug of the exercise which tests to run.
.PARAMETER Directory
    The directory in which the solution can be found.
.EXAMPLE
    The example below will run the tests of the two-fer solution in the "~/exercism/two-fer" directory
    PS C:\> ./run.ps1 two-fer ~/exercism/two-fer
#>

param (
    [Parameter(Position = 0, Mandatory = $true)]
    [string]$Exercise, 
    
    [Parameter(Position = 1, Mandatory = $true)]
    [string]$Directory
)

$ErrorActionPreference = 'Stop'

Get-ChildItem -Path $Directory -Include "*Test.cs" -Recurse | ForEach-Object {
    (Get-Content $_.FullName) -replace "Skip = ""Remove to run test""", "" | Set-Content $_.FullName
}

dotnet test $Directory

exit $LastExitCode