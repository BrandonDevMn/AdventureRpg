namespace AdventureRpgCli.Services;

public class SessionStore
{
    private readonly string _filePath;

    public SessionStore() : this(DefaultFilePath) { }

    internal SessionStore(string filePath)
    {
        _filePath = filePath;
    }

    private static readonly string DefaultFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".adventurerpg",
        "session");

    public string? Load()
    {
        try
        {
            if (!File.Exists(_filePath)) return null;
            var token = File.ReadAllText(_filePath).Trim();
            return string.IsNullOrEmpty(token) ? null : token;
        }
        catch
        {
            return null;
        }
    }

    public void Save(string refreshToken)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, refreshToken);
        }
        catch { /* best effort */ }
    }

    public void Clear()
    {
        try
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);
        }
        catch { /* best effort */ }
    }
}
