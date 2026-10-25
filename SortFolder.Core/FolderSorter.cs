namespace SortFolder.Core;

public record MovedFile(string FileName, string TargetFolder);

public record FileFailure(string FileName, string Reason);

public record SortResult(IReadOnlyList<MovedFile> Moved,
                         IReadOnlyList<FileFailure> Failures,
                         string? Error = null);

public class FolderSorter
{
    private const string DefaultTargetFolderName = "Others";

    private static readonly Dictionary<string, string> FileExtensions = new(StringComparer.OrdinalIgnoreCase)
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

    public SortResult Sort(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            return Fatal($"The folder '{folderPath}' is not a valid directory.");
        }

        string[] files;

        try
        {
            files = Directory.GetFiles(folderPath);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Fatal($"Access to '{folderPath}' is denied. {ex.Message}");
        }
        catch (DirectoryNotFoundException)
        {
            return Fatal($"The folder '{folderPath}' no longer exists.");
        }
        catch (PathTooLongException)
        {
            return Fatal($"The path '{folderPath}' is too long.");
        }
        catch (IOException ex)
        {
            return Fatal($"Could not read the folder '{folderPath}'. {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            return Fatal($"The path '{folderPath}' is invalid. {ex.Message}");
        }

        var moved = new List<MovedFile>();
        var failures = new List<FileFailure>();

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);

            try
            {
                var extension = Path.GetExtension(file);

                var targetFolderName = FileExtensions.TryGetValue(extension, out var mappedFolder)
                                     ? mappedFolder
                                     : DefaultTargetFolderName;

                var targetFolderPath = Path.Combine(folderPath, targetFolderName);

                Directory.CreateDirectory(targetFolderPath);

                var targetFilePath = GetUniqueFilePath(Path.Combine(targetFolderPath, fileName));

                File.Move(file, targetFilePath);

                moved.Add(new MovedFile(fileName, targetFolderName));
            }
            catch (FileNotFoundException)
            {
                failures.Add(new FileFailure(fileName, "The file no longer exists (deleted or moved by another process)."));
            }
            catch (DirectoryNotFoundException)
            {
                failures.Add(new FileFailure(fileName, "The target or source directory no longer exists."));
            }
            catch (PathTooLongException)
            {
                failures.Add(new FileFailure(fileName, "The resulting path is too long."));
            }
            catch (UnauthorizedAccessException ex)
            {
                failures.Add(new FileFailure(fileName, $"Access denied (read-only file or missing permissions). {ex.Message}"));
            }
            catch (IOException ex)
            {
                // Covers locked files, a file blocking the target folder name, disk/network errors.
                failures.Add(new FileFailure(fileName, $"I/O error (the file may be in use). {ex.Message}"));
            }
            catch (NotSupportedException ex)
            {
                failures.Add(new FileFailure(fileName, $"Unsupported path format. {ex.Message}"));
            }
            catch (ArgumentException ex)
            {
                failures.Add(new FileFailure(fileName, $"Invalid path. {ex.Message}"));
            }
        }

        return new SortResult(moved, failures);
    }

    private static SortResult Fatal(string error) => new([], [], error);

    // Returns the path unchanged if free, otherwise appends " (1)", " (2)", ... before the extension.
    private static string GetUniqueFilePath(string path)
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
}
