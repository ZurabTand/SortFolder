if (args.Length == 0)
{
    Console.WriteLine("Usage: SortFolder <folder_path> [--verbose]");

    return 1;
}

var folderPath = args[0];

if (!Path.Exists(folderPath))
{
    Console.WriteLine($"Error: The folder '{folderPath}' does not exist.");
    return 1;
}

if (!Directory.Exists(folderPath))
{
    Console.WriteLine($"Error: The folder '{folderPath}' is not a valid directory.");
    return 1;
}

Dictionary<string, string> fileExtensions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
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
    // Add more file extensions and their corresponding folder names as needed
};

foreach (var file in Directory.GetFiles(folderPath))
{
    var extension = Path.GetExtension(file);

    if (fileExtensions.TryGetValue(extension, out var targetFolderName))
    {
        var targetFolderPath = Path.Combine(folderPath, targetFolderName);

        Directory.CreateDirectory(targetFolderPath);

        var targetFilePath = Path.Combine(targetFolderPath, Path.GetFileName(file));

        File.Move(file, targetFilePath);
    }
    else
    {
        var targetFolderPath = Path.Combine(folderPath, "Others");

        Directory.CreateDirectory(targetFolderPath);

        var targetFilePath = Path.Combine(targetFolderPath, Path.GetFileName(file));

        File.Move(file, targetFilePath);
    }
}

return 0;