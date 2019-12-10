<#
.SYNOPSIS
    Run a solution's tests.
.DESCRIPTION
    Run a solution's tests.
.PARAMETER Exercise
    The slug of the exercise which tests to run.
.PARAMETER InputDirectory
    The directory in which the solution can be found.
.PARAMETER OutputDirectory
    The directory to which the results will be written.
.EXAMPLE
    The example below will run the tests of the two-fer solution in the "~/exercism/two-fer" directory
    and write the results to the "~/exercism/results/" directory
    PS C:\> ./run.ps1 two-fer ~/exercism/two-fer ~/exercism/results/
#>

param (
    [Parameter(Position = 0, Mandatory = $true)]
    [string]$Exercise,
    
    [Parameter(Position = 1, Mandatory = $true)]
    [string]$InputDirectory,
    
    [Parameter(Position = 2, Mandatory = $true)]
    [string]$OutputDirectory
)

dotnet run --project ./src/Exercism.TestRunner.CSharp/ $Exercise $InputDirectory $OutputDirectory