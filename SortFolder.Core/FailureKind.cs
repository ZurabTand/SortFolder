namespace SortFolder.Core;

/// <summary>What went wrong with one file. A UI can map this to its own (e.g. localized) text.</summary>
public enum FailureKind
{
    FileNotFound,

    DirectoryNotFound,

    PathTooLong,

    AccessDenied,

    IoError,

    UnsupportedPath,

    InvalidPath
}
