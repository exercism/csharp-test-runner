<#
.SYNOPSIS
    Run a solution's tests using the Docker test runner image.
.DESCRIPTION
    Run a solution's tests using the Docker test runner image.
    This script allows one to verify that this Docker image correctly
    runs the tests of a solution.
.PARAMETER Exercise
    The slug of the exercise which tests to run.
.PARAMETER Directory
    The directory in which the solution can be found.
.EXAMPLE
    The example below will run the tests of the two-fer solution in the "~/exercism/two-fer" directory
    PS C:\> ./run-in-docker.ps1 two-fer ~/exercism/two-fer
#>

param (
    [Parameter(Position = 0, Mandatory = $true)]
    [string]$Exercise, 
    
    [Parameter(Position = 1, Mandatory = $true)]
    [string]$Directory
)

docker build -t exercism/csharp-test-runner .
docker run -v ${Directory}:/solution exercism/csharp-test-runner $Exercise /solution