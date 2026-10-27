namespace SortFolder.Core;

public class FolderSorter
{
    private readonly Dictionary<string, string> _rules;

    private readonly string _otherFolderName;

    /// <param name="rules">Extension (e.g. ".jpg") to target folder name. Defaults to <see cref="SortRules.Default"/>.</param>
    /// <param name="otherFolderName">Folder for files whose extension has no rule. Defaults to "Others".</param>
    /// <exception cref="ArgumentException">A folder name is empty or not a valid single folder name.</exception>
    public FolderSorter(IReadOnlyDictionary<string, string>? rules = null, string? otherFolderName = null)
    {
        _otherFolderName = otherFolderName ?? SortRules.DefaultOtherFolderName;

        if (!SortRules.IsValidFolderName(_otherFolderName))
        {
            throw new ArgumentException($"'{_otherFolderName}' is not a valid folder name.", nameof(otherFolderName));
        }

        _rules = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (extension, folder) in rules ?? SortRules.Default)
        {
            if (!SortRules.IsValidFolderName(folder))
            {
                throw new ArgumentException($"'{folder}' is not a valid folder name.", nameof(rules));
            }

            _rules[extension] = folder;
        }
    }

    public SortResult Sort(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            return Fatal(SortErrorKind.InvalidFolder, $"The folder '{folderPath}' is not a valid directory.");
        }

        string[] files;

        try
        {
            files = Directory.GetFiles(folderPath);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Fatal(SortErrorKind.AccessDenied, $"Access to '{folderPath}' is denied. {ex.Message}", ex.Message);
        }
        catch (DirectoryNotFoundException)
        {
            return Fatal(SortErrorKind.FolderNotFound, $"The folder '{folderPath}' no longer exists.");
        }
        catch (PathTooLongException)
        {
            return Fatal(SortErrorKind.PathTooLong, $"The path '{folderPath}' is too long.");
        }
        catch (IOException ex)
        {
            return Fatal(SortErrorKind.Unreadable, $"Could not read the folder '{folderPath}'. {ex.Message}", ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Fatal(SortErrorKind.InvalidPath, $"The path '{folderPath}' is invalid. {ex.Message}", ex.Message);
        }

        var moved = new List<MovedFile>();
        var failures = new List<FileFailure>();

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);

            try
            {
                var extension = Path.GetExtension(file);

                var targetFolderName = _rules.TryGetValue(extension, out var mappedFolder)
                                     ? mappedFolder
                                     : _otherFolderName;

                var targetFolderPath = Path.Combine(folderPath, targetFolderName);

                Directory.CreateDirectory(targetFolderPath);

                var targetFilePath = GetUniqueFilePath(Path.Combine(targetFolderPath, fileName));

                File.Move(file, targetFilePath);

                moved.Add(new MovedFile(fileName, targetFolderName));
            }
            catch (FileNotFoundException)
            {
                failures.Add(new FileFailure(fileName, FailureKind.FileNotFound));
            }
            catch (DirectoryNotFoundException)
            {
                failures.Add(new FileFailure(fileName, FailureKind.DirectoryNotFound));
            }
            catch (PathTooLongException)
            {
                failures.Add(new FileFailure(fileName, FailureKind.PathTooLong));
            }
            catch (UnauthorizedAccessException ex)
            {
                failures.Add(new FileFailure(fileName, FailureKind.AccessDenied, ex.Message));
            }
            catch (IOException ex)
            {
                // Covers locked files, a file blocking the target folder name, disk/network errors.
                failures.Add(new FileFailure(fileName, FailureKind.IoError, ex.Message));
            }
            catch (NotSupportedException ex)
            {
                failures.Add(new FileFailure(fileName, FailureKind.UnsupportedPath, ex.Message));
            }
            catch (ArgumentException ex)
            {
                failures.Add(new FileFailure(fileName, FailureKind.InvalidPath, ex.Message));
            }
        }

        return new SortResult(moved, failures);
    }

    private static SortResult Fatal(SortErrorKind kind, string error, string detail = "") => new([], [], error, kind, detail);

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
