namespace SortFolder.Core;

/// <summary>What went wrong with the folder as a whole.</summary>
public enum SortErrorKind
{
    InvalidFolder,

    AccessDenied,

    FolderNotFound,

    PathTooLong,

    Unreadable,

    InvalidPath
}
