if (args.Length == 0)
{
    Console.WriteLine("Usage: SortFolder <folder_path> [--verbose]");

    return 1;
}

var folderPath = args[0];
var verbose = args.Skip(1).Contains("--verbose", StringComparer.OrdinalIgnoreCase);

if (!Directory.Exists(folderPath))
{
    Console.WriteLine($"Error: The folder '{folderPath}' is not a valid directory.");

    return 1;
}

var fileExtensions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    { ".txt", "TextFiles" },
    { ".jpg", "Images" },
    { ".jpeg", "Images" },
    { ".png", "Images" },
    { ".gif", "Images" },
    { ".bmp", "Images" },
    { ".pdf", "Documents" },
    { ".doc", "Documents" },
    { ".docx", "Documents" },
    { ".xls", "Spreadsheets" },
    { ".xlsx", "Spreadsheets" },
    { ".ppt", "Presentations" },
    { ".pptx", "Presentations" },
    { ".mp3", "Audio" },
    { ".wav", "Audio" },
    { ".mp4", "Videos" },
    { ".avi", "Videos" },
    { ".mkv", "Videos" },
    { ".zip", "Archives" },
    { ".rar", "Archives" },
    { ".7z", "Archives" }
};

// Listing the folder is fatal if it fails: there is nothing to process.
string[] files;

try
{
    files = Directory.GetFiles(folderPath);
}
catch (UnauthorizedAccessException ex)
{
    Console.WriteLine($"Error: Access to '{folderPath}' is denied. {ex.Message}");
    return 2;
}
catch (DirectoryNotFoundException)
{
    Console.WriteLine($"Error: The folder '{folderPath}' no longer exists.");
    return 2;
}
catch (PathTooLongException)
{
    Console.WriteLine($"Error: The path '{folderPath}' is too long.");
    return 2;
}
catch (IOException ex)
{
    Console.WriteLine($"Error: Could not read the folder '{folderPath}'. {ex.Message}");
    return 2;
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Error: The path '{folderPath}' is invalid. {ex.Message}");
    return 2;
}

var movedCount = 0;
var failedCount = 0;

foreach (var file in files)
{
    var fileName = Path.GetFileName(file);

    try
    {
        var extension = Path.GetExtension(file);

        var targetFolderName = fileExtensions.TryGetValue(extension, out var mappedFolder)
                             ? mappedFolder
                             : "Others";

        var targetFolderPath = Path.Combine(folderPath, targetFolderName);

        Directory.CreateDirectory(targetFolderPath);

        var targetFilePath = GetUniqueFilePath(Path.Combine(targetFolderPath, fileName));

        File.Move(file, targetFilePath);

        movedCount++;

        if (verbose)
        {
            Console.WriteLine($"Moved '{fileName}' -> '{targetFolderName}'");
        }
    }
    catch (FileNotFoundException)
    {
        ReportFailure(fileName, "The file no longer exists (deleted or moved by another process).");
    }
    catch (DirectoryNotFoundException)
    {
        ReportFailure(fileName, "The target or source directory no longer exists.");
    }
    catch (PathTooLongException)
    {
        ReportFailure(fileName, "The resulting path is too long.");
    }
    catch (UnauthorizedAccessException ex)
    {
        ReportFailure(fileName, $"Access denied (read-only file or missing permissions). {ex.Message}");
    }
    catch (IOException ex)
    {
        // Covers locked files, a file blocking the target folder name, disk/network errors.
        ReportFailure(fileName, $"I/O error (the file may be in use). {ex.Message}");
    }
    catch (NotSupportedException ex)
    {
        ReportFailure(fileName, $"Unsupported path format. {ex.Message}");
    }
    catch (ArgumentException ex)
    {
        ReportFailure(fileName, $"Invalid path. {ex.Message}");
    }
}

Console.WriteLine($"Done. Moved: {movedCount}, Failed: {failedCount}.");

return failedCount == 0 ? 0 : 3;

static string GetUniqueFilePath(string path)
{
    if (!File.Exists(path))
    {
        return path;
    }

    var directory = Path.GetDirectoryName(path)!;
    var nameWithoutExtension = Path.GetFileNameWithoutExtension(path);
    var extension = Path.GetExtension(path);

    for (var i = 1; ; i++)
    {
        var candidate = Path.Combine(directory, $"{nameWithoutExtension} ({i}){extension}");

        if (!File.Exists(candidate))
        {
            return candidate;
        }
    }
}

void ReportFailure(string fileName, string reason)
{
    failedCount++;

    Console.WriteLine($"Skipped '{fileName}': {reason}");
}
