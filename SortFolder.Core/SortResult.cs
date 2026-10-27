namespace SortFolder.Core;

/// <summary>
/// Outcome of a sort run. When the folder itself could not be processed, <see cref="Error"/> holds an
/// English message and <see cref="ErrorKind"/> / <see cref="ErrorDetail"/> let a UI build its own.
/// </summary>
public record SortResult(IReadOnlyList<MovedFile> Moved,
                         IReadOnlyList<FileFailure> Failures,
                         string? Error = null,
                         SortErrorKind? ErrorKind = null,
                         string ErrorDetail = "");
