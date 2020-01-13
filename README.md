# Exercism C# test runner

A test runner automatically verifies if a submission passes all the tests.

This repository contains the C# test runner, which implements the [test runner interface][test-runner-interface].

## Running the tests

To run a solution's tests, follow these steps:

1. Open a command prompt in the root directory.
1. Run `./run.ps1 <exercise> <input-directory> <output-directory>`. This script will:
   1. Make sure all tests run (no skipped tests) for the solution found in `<input-directory>`.
   1. Run all the tests.
   1. Once the script has completed, the test results will be written to `<output-directory>/results.json`.

## Running the tests using Docker

To run a solution's tests using a Docker container, follow these steps:

1. Open a command prompt in the root directory.
1. Run `./run-in-docker.ps1 <exercise> <input-directory> <output-directory>`. This script will:
   1. Make sure all tests run (no skipped tests) for the solution found in `<input-directory>`.
   1. Run all the tests.
   1. Once the script has completed, the test results will be written to `<output-directory>/results.json`.

### Scripts

The scripts in this repository are written in PowerShell. As PowerShell is cross-platform nowadays, you can also install it on [Linux](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-linux?view=powershell-6) and [macOS](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-macos?view=powershell-6).

[test-runner-interface]: https://github.com/exercism/automated-tests/blob/master/docs/interface.md
