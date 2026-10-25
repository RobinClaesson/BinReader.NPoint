using NSubstitute;
using NPoint.Net.Caching;

namespace NPoint.Net.Tests;

public class NPointFileSystemCacheTests
{
    private const string CachePath = "CachePath";
    private static string GetRandomDocumentId() => Guid.NewGuid().ToString("N")[..20];

    [Fact]
    public async Task ContainsAsync_WhenFileDoesNotExist_ReturnsFalse()
    {
        var documentId = GetRandomDocumentId();
        var expectedFilePath = $"{CachePath}/{documentId}.json";

        var mockFileSystem = Substitute.For<IFileSystem>();
        mockFileSystem.CachePath.Returns(CachePath);
        mockFileSystem.FileExists(expectedFilePath).Returns(false);
        NPointFileSystemCache.FileSystem = mockFileSystem;

        var target = new NPointFileSystemCache();

        var result = await target.ContainsAsync(documentId);

        Assert.False(result);

        mockFileSystem.Received(1).FileExists(expectedFilePath);
        mockFileSystem.DidNotReceive().GetLastWriteTimeUtc(Arg.Any<string>());
    }

    [Fact]
    public async Task ContainsAsync_WhenFileWasCreatedLongerThanTtlAgo_ReturnsFalse()
    {
        var documentId = GetRandomDocumentId();
        var expectedFilePath = $"{CachePath}/{documentId}.json";
        var fileCreationTimeUtc = (DateTimeOffset.UtcNow - TimeSpan.FromSeconds(11)).UtcDateTime;

        var mockFileSystem = Substitute.For<IFileSystem>();
        mockFileSystem.CachePath.Returns(CachePath);
        mockFileSystem.FileExists(expectedFilePath).Returns(true);
        mockFileSystem.GetLastWriteTimeUtc(expectedFilePath).Returns(fileCreationTimeUtc);
        NPointFileSystemCache.FileSystem = mockFileSystem;

        var target = new NPointFileSystemCache
        {
            Ttl = TimeSpan.FromSeconds(10)
        };

        var result = await target.ContainsAsync(documentId);
        Assert.False(result);

        mockFileSystem.Received(1).FileExists(expectedFilePath);
        mockFileSystem.Received(1).GetLastWriteTimeUtc(expectedFilePath);
    }

    [Fact]
    public async Task ContainsAsync_WhenFileWasCreatedLessThanTtlAgo_ReturnsTrue()
    {
        var documentId = GetRandomDocumentId();
        var expectedFilePath = $"{CachePath}/{documentId}.json";
        var fileCreationTimeUtc = (DateTimeOffset.UtcNow - TimeSpan.FromSeconds(9)).UtcDateTime;

        var mockFileSystem = Substitute.For<IFileSystem>();
        mockFileSystem.CachePath.Returns(CachePath);
        mockFileSystem.FileExists(expectedFilePath).Returns(true);
        mockFileSystem.GetLastWriteTimeUtc(expectedFilePath).Returns(fileCreationTimeUtc);
        NPointFileSystemCache.FileSystem = mockFileSystem;

        var target = new NPointFileSystemCache
        {
            Ttl = TimeSpan.FromSeconds(10)
        };

        var result = await target.ContainsAsync(documentId);
        Assert.True(result);

        mockFileSystem.Received(1).FileExists(expectedFilePath);
        mockFileSystem.Received(1).GetLastWriteTimeUtc(expectedFilePath);
    }

    [Fact]
    public async Task GetAsync_WhenFileDoesNotExist_ReturnsNull()
    {
        var documentId = GetRandomDocumentId();
        var expectedFilePath = $"{CachePath}/{documentId}.json";

        var mockFileSystem = Substitute.For<IFileSystem>();
        mockFileSystem.CachePath.Returns(CachePath);
        mockFileSystem.FileExists(expectedFilePath).Returns(false);
        NPointFileSystemCache.FileSystem = mockFileSystem;

        var target = new NPointFileSystemCache();

        var result = await target.GetAsync(documentId);

        Assert.Null(result);

        mockFileSystem.Received(1).FileExists(expectedFilePath);
        mockFileSystem.DidNotReceive().GetLastWriteTimeUtc(Arg.Any<string>());
        await mockFileSystem.DidNotReceive().ReadAllTextAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GetAsync_WhenFileWasCreatedLongerThanTtlAgo_ReturnsNull()
    {
        var documentId = GetRandomDocumentId();
        var expectedFilePath = $"{CachePath}/{documentId}.json";
        var fileCreationTimeUtc = (DateTimeOffset.UtcNow - TimeSpan.FromSeconds(11)).UtcDateTime;

        var mockFileSystem = Substitute.For<IFileSystem>();
        mockFileSystem.CachePath.Returns(CachePath);
        mockFileSystem.FileExists(expectedFilePath).Returns(true);
        mockFileSystem.GetLastWriteTimeUtc(expectedFilePath).Returns(fileCreationTimeUtc);
        NPointFileSystemCache.FileSystem = mockFileSystem;

        var target = new NPointFileSystemCache
        {
            Ttl = TimeSpan.FromSeconds(10)
        };

        var result = await target.GetAsync(documentId);
        Assert.Null(result);

        mockFileSystem.Received(1).FileExists(expectedFilePath);
        mockFileSystem.Received(1).GetLastWriteTimeUtc(expectedFilePath);
        await mockFileSystem.DidNotReceive().ReadAllTextAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GetAsync_WhenFileWasCreatedLessThanTtlAgo_ReturnsFileContents()
    {
        var documentId = GetRandomDocumentId();
        var expectedFilePath = $"{CachePath}/{documentId}.json";
        var fileCreationTimeUtc = (DateTimeOffset.UtcNow - TimeSpan.FromSeconds(9)).UtcDateTime;
        const string fileContent = """
                                   { "Foo": "bar" }
                                   """;

        var mockFileSystem = Substitute.For<IFileSystem>();
        mockFileSystem.CachePath.Returns(CachePath);
        mockFileSystem.FileExists(expectedFilePath).Returns(true);
        mockFileSystem.GetLastWriteTimeUtc(expectedFilePath).Returns(fileCreationTimeUtc);
        mockFileSystem.ReadAllTextAsync(expectedFilePath).Returns(fileContent);
        NPointFileSystemCache.FileSystem = mockFileSystem;

        var target = new NPointFileSystemCache
        {
            Ttl = TimeSpan.FromSeconds(10)
        };

        var result = await target.GetAsync(documentId);
        Assert.NotNull(result);
        Assert.Equal(fileContent, result);

        mockFileSystem.Received(1).FileExists(expectedFilePath);
        mockFileSystem.Received(1).GetLastWriteTimeUtc(expectedFilePath);
        await mockFileSystem.Received(1).ReadAllTextAsync(expectedFilePath);
    }

    [Fact]
    public async Task SetAsync_WritesAllTextToFile()
    {
        var documentId = GetRandomDocumentId();
        var expectedFilePath = $"{CachePath}/{documentId}.json";
        const string setContent = """
                                  { "Foo": "bar" }
                                  """;

        var mockFileSystem = Substitute.For<IFileSystem>();
        mockFileSystem.CachePath.Returns(CachePath);
        NPointFileSystemCache.FileSystem = mockFileSystem;

        var target = new NPointFileSystemCache();

        await target.SetAsync(documentId, setContent);

        await mockFileSystem.Received(1).WriteAllTextAsync(expectedFilePath, setContent);
    }

    [Fact]
    public async Task ClearAsync_DeletesAllCacheFiles()
    {
        var cacheFiles = new List<string>
        {
            $"{GetRandomDocumentId()}.json", $"{GetRandomDocumentId()}.json", $"{GetRandomDocumentId()}.json"
        };

        var mockFileSystem = Substitute.For<IFileSystem>();
        mockFileSystem.CachePath.Returns(CachePath);
        mockFileSystem.GetDirectoryFiles(CachePath).Returns(cacheFiles);
        NPointFileSystemCache.FileSystem = mockFileSystem;

        var target = new NPointFileSystemCache();

        await target.ClearAsync();

        mockFileSystem.Received(1).GetDirectoryFiles(CachePath);
        mockFileSystem.Received(3).DeleteFile(Arg.Any<string>());
        cacheFiles.ForEach(mockFileSystem.Received(1).DeleteFile);
    }
}