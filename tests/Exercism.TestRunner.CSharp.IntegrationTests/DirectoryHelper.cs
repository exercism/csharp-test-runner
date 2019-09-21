using System;
using System.IO;
using System.Linq;

namespace Exercism.TestRunner.CSharp.IntegrationTests
{
    internal static class DirectoryHelper
    {
        public static string FindFileInTree(string file)
        {
            bool IsMatchingFile(string fileInDirectory) => Path.GetFileName(fileInDirectory) == file;

            var directory = Directory.GetCurrentDirectory();

            while (directory != null)
            {
                var matchingFilePath = Directory.EnumerateFiles(directory).FirstOrDefault(IsMatchingFile);
                if (matchingFilePath != null)
                    return matchingFilePath;

                directory = Directory.GetParent(directory).FullName;
            }

            throw new InvalidOperationException("Could not find run.ps1 file");
        }
    }
}