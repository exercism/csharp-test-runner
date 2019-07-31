# Exercism C# test runner

The Docker image for running C# solutions submitted to [Exercism](https://exercism.io).

## Running the tests

To run a solution's tests, follow these steps:

1. Open a command prompt in the root directory.
1. Run `./run.ps1 <exercise> <directory>`. This script will:
   1. Make sure all tests run (no skipped tests).
   1. Run all the tests.

## Running the tests using Docker

To run a solution's tests using a Docker container, follow these steps:

1. Open a command prompt in the root directory.
1. Run `./run-in-docker.ps1 <exercise> <directory>`. This script will:
   1. Make sure all tests run (no skipped tests).
   1. Run all the tests.

### Scripts

The scripts in this repository are written in PowerShell. As PowerShell is cross-platform nowadays, you can also install it on [Linux](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-linux?view=powershell-6) and [macOS](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-macos?view=powershell-6).
