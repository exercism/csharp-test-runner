#!/usr/bin/env sh

# Synopsis:
# Run the test runner on a solution.

# Arguments:
# $1: exercise slug
# $2: path to solution folder
# $3: path to output directory

# Output:
# Writes the test results to a results.json file in the passed-in output directory.
# The test results are formatted according to the specifications at https://github.com/exercism/docs/blob/main/building/tooling/test-runners/interface.md

# Example:
# ./bin/run.sh two-fer path/to/solution/folder/ path/to/output/directory/

# If any required arguments is missing, print the usage and exit
if [ "$#" -lt 3 ]; then
    echo "usage: ./bin/run.sh exercise-slug path/to/solution/folder/ path/to/output/directory/"
    exit 1
fi

if [ -f /opt/test-runner/Exercism.TestRunner.CSharp ]; then
    /opt/test-runner/Exercism.TestRunner.CSharp $1 $2 $3
else
    dotnet run --project ./src/Exercism.TestRunner.CSharp/ $1 $2 $3
fi
