using CommandLine;
using Serilog;

namespace Exercism.TestRunner.CSharp
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Logging.Configure();

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(CreateRepresentation);
        }

        private static void CreateRepresentation(Options options)
        {
            Log.Information("Creating representation for {Exercise} solution in directory {Directory}", options.Slug, options.InputDirectory);

//            var solution = SolutionParser.Parse(options);
//            var (representation, mapping) = SolutionRepresenter.Represent(solution);

            MappingWriter.WriteToFile(options);

            Log.Information("Created representation for {Exercise} solution in directory {Directory}", options.Slug, options.OutputDirectory);
        }
    }
}