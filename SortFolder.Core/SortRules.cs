namespace SortFolder.Core;

public static class SortRules
{
    public const string DefaultOtherFolderName = "Others";

    public static IReadOnlyDictionary<string, string> Default { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
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
            { ".7z", "Archives" },
            { ".exe", "Executables" }
        };

    public static bool IsValidFolderName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || name != name.Trim() || name.EndsWith('.'))
        {
            return false;
        }

        return name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 && !name.Contains('\\');
    }

    public static bool TryNormalizeExtension(string? input, out string extension)
    {
        extension = string.Empty;

        var text = input?.Trim();

        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        if (!text.StartsWith('.'))
        {
            text = "." + text;
        }

        var body = text[1..];

        if (body.Length == 0 || 
            body.Contains('.') || 
            body.Any(char.IsWhiteSpace) ||
            body.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0|| 
            body.Contains('\\'))
        {
            return false;
        }

        extension = text.ToLowerInvariant();

        return true;
    }
}
