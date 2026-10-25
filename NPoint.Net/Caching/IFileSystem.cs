namespace NPoint.Net.Caching;

internal interface IFileSystem
{
    bool FileExists(string? path);
    void DeleteFile(string path);
    DateTime GetLastWriteTimeUtc(string path);
    Task<string> ReadAllTextAsync(string path);
    Task WriteAllTextAsync(string path, string contents);
    bool DirectoryExists(string path);
    void CreateDirectory(string path);
    List<string> GetDirectoryFiles(string path);
    string CachePath { get; }
}

internal class FileSystem : IFileSystem
{
    public bool FileExists(string? path) => File.Exists(path);
    public void DeleteFile(string path) => File.Delete(path);
    public DateTime GetLastWriteTimeUtc(string path) => File.GetLastWriteTime(path);
    public Task<string> ReadAllTextAsync(string path) => File.ReadAllTextAsync(path);
    public Task WriteAllTextAsync(string path, string contents) => File.WriteAllTextAsync(path, contents);
    public bool DirectoryExists(string path) => Directory.Exists(path);
    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
    public List<string> GetDirectoryFiles(string path) => [.. Directory.GetFiles(path)];

    public string CachePath => Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CalcTools"
    );
}