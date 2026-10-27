namespace SortFolder.Core;

/// <summary>
/// A file that could not be moved. <see cref="Detail"/> is the system's own message, if any;
/// <see cref="Reason"/> is a ready-made English description for console output.
/// </summary>
public record FileFailure(string FileName, FailureKind Kind, string Detail = "")
{
    public string Reason => Kind switch
    {
        FailureKind.FileNotFound => "The file no longer exists (deleted or moved by another process).",
        FailureKind.DirectoryNotFound => "The target or source directory no longer exists.",
        FailureKind.PathTooLong => "The resulting path is too long.",
        FailureKind.AccessDenied => $"Access denied (read-only file or missing permissions). {Detail}".TrimEnd(),
        FailureKind.IoError => $"I/O error (the file may be in use). {Detail}".TrimEnd(),
        FailureKind.UnsupportedPath => $"Unsupported path format. {Detail}".TrimEnd(),
        FailureKind.InvalidPath => $"Invalid path. {Detail}".TrimEnd(),
        _ => Detail
    };
}
