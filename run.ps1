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

$ErrorActionPreference = 'Stop'

function Get-Exercise-Name {
    (Get-Culture).TextInfo.ToTitleCase($Exercise).Replace("-", "")
}

$exerciseName = Get-Exercise-Name

$testFile = Join-Path $InputDirectory "${exerciseName}Test.cs"
$resultsTrxFile = Join-Path $InputDirectory "TestResults" "results.trx"
$resultsJsonFile = Join-Path $OutputDirectory "results.json"

filter Unskip-All-Tests {
    $_ -Replace "Skip = ""Remove to run test""", ""
}

function Enable-All-Tests {
    $content = Get-Content $testFile | Unskip-All-Tests
    $content | Set-Content $testFile
}

filter Sanitize-Test-Output {
    $sanitized = $_ -Replace "\s*\[$InputDirectory.*?\]", ""
    $sanitized = $sanitized -Replace "^.+?\.cs\(\d+,\d+\):\s*", ""
    $sanitized = $sanitized -Replace "\r?\n.+?\.cs\(\d+,\d+\):\s*", "`n"
    $sanitized.Trim()
}

function Run-All-Tests {
    Remove-Item $resultsTrxFile -ErrorAction Ignore

    $logger = "trx;LogFileName=results.trx"
    dotnet test $InputDirectory --logger:$logger | Out-String | Sanitize-Test-Output
}

function Get-Tests-Ordered-By-Line-Number {
    $testFileContents = Get-Content $testFile
    $testMethodRegex = "public.*?([\w_]+)\("
    $testMethodMatches = [regex]::Matches($testFileContents, $testMethodRegex)

    $orderedTestMethods = @()

    foreach ($captures in $testMethodMatches.Captures) {
        $testMethodName = $captures.Groups[1].Value
        $fullMethodName = "${exerciseName}Test.${testMethodName}";
        $orderedTestMethods += $fullMethodName
    }

    $orderedTestMethods
}

function Sort-Unit-Test-Results-By-Line-Number ($UnitTestResults) {
    $orderedTestMethods = Get-Tests-Ordered-By-Line-Number
    $UnitTestResults | Sort-Object { $orderedTestMethods.IndexOf($_.testName) }
}

function Get-Unit-Test-Results {
    [xml]$resultsTrxFileXml = Get-Content $resultsTrxFile
    $unitTestResults = $resultsTrxFileXml.TestRun.Results.UnitTestResult
    Sort-Unit-Test-Results-By-Line-Number $unitTestResults
}

function Create-Test-Result-For-Passed-Test ($UnitTestResult) {
    [pscustomobject]@{
        name   = $UnitTestResult.testName;
        status = "pass";
    }
}

filter Sanitize-Error-Message {
    $_ -Replace "System.NotImplementedException\s*:\s*", ""
}

function Create-Test-Result-For-Failed-Test ($UnitTestResult) {
    [pscustomobject]@{
        name    = $UnitTestResult.testName;
        status  = "fail";
        message = $UnitTestResult.Output.ErrorInfo.Message | Sanitize-Error-Message;
    }
}

function Create-Test-Results-For-Successful-Build {
    $status = "pass"
    $tests = @()

    foreach ($unitTestResult in Get-Unit-Test-Results) {
        if ($unitTestResult.outcome -eq "Passed") {
            $tests += Create-Test-Result-For-Passed-Test $unitTestResult
        }
        elseif ($unitTestResult.outcome -eq "Failed") {
            $status = "fail"
            $tests += Create-Test-Result-For-Failed-Test $unitTestResult
        }
    }

    [pscustomobject]@{
        status = $status;
        tests  = $tests;
    }
}

function Create-Test-Results-For-Failed-Build ([string]$TestOutput) {
    [pscustomobject]@{
        status  = "error";
        message = $TestOutput
        tests   = @();
    }
}

function Create-Test-Results ([string]$TestOutput) {
    if (Test-Path $resultsTrxFile) {
        Create-Test-Results-For-Successful-Build
    }
    else {
        Create-Test-Results-For-Failed-Build $TestOutput
    }
}

function Write-Test-Results-To-File ([string]$TestOutput) {
    Create-Test-Results $TestOutput | ConvertTo-Json | Set-Content -Path $resultsJsonFile
}

Enable-All-Tests
$testOutput = Run-All-Tests
Write-Test-Results-To-File $testOutput

exit $LastExitCode