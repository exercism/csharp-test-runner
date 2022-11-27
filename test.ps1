<#
.SYNOPSIS
    Run all tests.
.DESCRIPTION
    Run all tests, verifying the behavior of the test runner.
.PARAMETER UpdateExpected
    Update the expected test result files to the current output (optional).
.EXAMPLE
    The example below will run all tests
    PS C:\> ./test.ps1

    The example below will run all tests and update the expected test result files
    PS C:\> ./test.ps1 -UpdateExpected
.NOTES
    The UpdateExpected switch should only be used if a bulk update of the expected test result files is needed.
#>

param (
    [Parameter(Mandatory = $false)]
    [Switch]$UpdateExpected,

    [Parameter(Mandatory = $false)]
    [Switch]$UseDocker
)

function Run-Test-Runner ([string] $SolutionDir) {
    $slug = (Get-ChildItem -Path $SolutionDir -Filter *.csproj).BaseName
    ./run.ps1 $slug $SolutionDir $SolutionDir
}

function Move-Generated-Test-Results-To-Expected ([string] $SolutionsDir) {
    $resultsFile = Join-Path $SolutionsDir "results.json"
    $expectedResultsFile = Join-Path $SolutionsDir "expected_results.json"
    Move-Item -Force $resultsFile $expectedResultsFile
}

function Update-Expected {
    $solutionsDir = "tests"
    
    Get-ChildItem $solutionsDir -Directory | ForEach-Object { 
        Run-Test-Runner $_.FullName
        Move-Generated-Test-Results-To-Expected $_.FullName
    }
}

if ($UpdateExpected.IsPresent) {
    Update-Expected
}

dotnet test

exit $LastExitCode