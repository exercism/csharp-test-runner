<#
.SYNOPSIS
    Format the C# source code.
.DESCRIPTION
    Formats the C# source code.
.EXAMPLE
    The example below will format all C# source code
    PS C:\> ./format.ps1
#>

dotnet tool restore
dotnet format