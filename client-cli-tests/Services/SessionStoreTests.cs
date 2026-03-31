using AdventureRpgCli.Services;
using Xunit;

namespace client_cli_tests.Services;

public class SessionStoreTests : IDisposable
{
    private readonly string _filePath = Path.Combine(Path.GetTempPath(), $"session-test-{Guid.NewGuid()}");
    private SessionStore Store => new(_filePath);

    [Fact]
    public void Load_FileDoesNotExist_ReturnsNull()
    {
        var result = Store.Load();

        Assert.Null(result);
    }

    [Fact]
    public void Load_FileIsEmpty_ReturnsNull()
    {
        File.WriteAllText(_filePath, "   ");

        var result = Store.Load();

        Assert.Null(result);
    }

    [Fact]
    public void Save_ThenLoad_ReturnsToken()
    {
        Store.Save("my-refresh-token");

        var result = Store.Load();

        Assert.Equal("my-refresh-token", result);
    }

    [Fact]
    public void Clear_AfterSave_LoadReturnsNull()
    {
        Store.Save("my-refresh-token");
        Store.Clear();

        var result = Store.Load();

        Assert.Null(result);
    }

    [Fact]
    public void Clear_WhenFileDoesNotExist_DoesNotThrow()
    {
        var exception = Record.Exception(() => Store.Clear());

        Assert.Null(exception);
    }

    public void Dispose()
    {
        if (File.Exists(_filePath))
            File.Delete(_filePath);
    }
}
