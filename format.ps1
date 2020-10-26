<#
.SYNOPSIS
    Format the source code.
.DESCRIPTION
    Formats the .NET source code.
.EXAMPLE
    The example below will format all source code
    PS C:\> ./format.ps1
#>

dotnet tool restore
dotnet format