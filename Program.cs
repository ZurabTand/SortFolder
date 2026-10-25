using SortFolder.Core;

namespace SortFolder;

internal class Program
{
    private const int ExitSuccess = 0;

    private const int ExitUsageError = 1;

    private const int ExitFolderError = 2;

    private const int ExitSomeFilesFailed = 3;

    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: SortFolder <folder_path> [--verbose]");
            Console.WriteLine("  --verbose  Print every moved file.");

            return ExitUsageError;
        }

        var folderPath = args[0];
        var verbose = args.Skip(1).Contains("--verbose", StringComparer.OrdinalIgnoreCase);

        var result = new FolderSorter().Sort(folderPath);

        if (result.Error is not null)
        {
            Console.WriteLine($"Error: {result.Error}");

            return ExitFolderError;
        }

        if (verbose)
        {
            foreach (var moved in result.Moved)
            {
                Console.WriteLine($"Moved '{moved.FileName}' -> '{moved.TargetFolder}'");
            }
        }

        foreach (var failure in result.Failures)
        {
            Console.WriteLine($"Skipped '{failure.FileName}': {failure.Reason}");
        }

        Console.WriteLine($"Done. Moved: {result.Moved.Count}, Failed: {result.Failures.Count}.");

        return result.Failures.Count == 0 ? ExitSuccess : ExitSomeFilesFailed;
    }
}
