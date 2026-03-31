namespace AdventureRpgCli.Services;

public class SessionStore
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".adventurerpg",
        "session");

    public string? Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return null;
            var token = File.ReadAllText(FilePath).Trim();
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
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, refreshToken);
        }
        catch { /* best effort */ }
    }

    public void Clear()
    {
        try
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
        catch { /* best effort */ }
    }
}
