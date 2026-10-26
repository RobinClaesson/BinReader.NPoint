using NSubstitute;
using BinReader.NPoint.Caching;

namespace BinReader.NPoint.Tests;

[Collection(nameof(TestFixture))]
public class NPointFileSystemCacheTests(TestFixture fixture) : IClassFixture<TestFixture>, IDisposable
{
    public void Dispose() => fixture.FileSystem.ClearReceivedCalls();

    [Fact]
    public async Task ContainsAsync_WhenFileDoesNotExist_ReturnsFalse()
    {
        var documentId = TestFixture.GetRandomDocumentId();
        var expectedFilePath = TestFixture.GetExpectedFilePath(documentId);

        fixture.FileSystem.FileExists(expectedFilePath).Returns(false);

        var target = new NPointFileSystemCache();

        var result = await target.ContainsAsync(documentId);
        Assert.False(result);

        fixture.FileSystem.Received(1).FileExists(expectedFilePath);
        fixture.FileSystem.DidNotReceive().GetLastWriteTimeUtc(Arg.Any<string>());
    }

    [Fact]
    public async Task ContainsAsync_WhenFileWasCreatedLongerThanTtlAgo_ReturnsFalse()
    {
        var documentId = TestFixture.GetRandomDocumentId();
        var expectedFilePath = TestFixture.GetExpectedFilePath(documentId);
        var fileCreationTimeUtc = (DateTimeOffset.UtcNow - TimeSpan.FromSeconds(11)).UtcDateTime;

        fixture.FileSystem.FileExists(expectedFilePath).Returns(true);
        fixture.FileSystem.GetLastWriteTimeUtc(expectedFilePath).Returns(fileCreationTimeUtc);

        var target = new NPointFileSystemCache
        {
            Ttl = TimeSpan.FromSeconds(10)
        };

        var result = await target.ContainsAsync(documentId);
        Assert.False(result);

        fixture.FileSystem.Received(1).FileExists(expectedFilePath);
        fixture.FileSystem.Received(1).GetLastWriteTimeUtc(expectedFilePath);
    }

    [Fact]
    public async Task ContainsAsync_WhenFileWasCreatedLessThanTtlAgo_ReturnsTrue()
    {
        var documentId = TestFixture.GetRandomDocumentId();
        var expectedFilePath = TestFixture.GetExpectedFilePath(documentId);
        var fileCreationTimeUtc = (DateTimeOffset.UtcNow - TimeSpan.FromSeconds(9)).UtcDateTime;

        fixture.FileSystem.FileExists(expectedFilePath).Returns(true);
        fixture.FileSystem.GetLastWriteTimeUtc(expectedFilePath).Returns(fileCreationTimeUtc);

        var target = new NPointFileSystemCache
        {
            Ttl = TimeSpan.FromSeconds(10)
        };

        var result = await target.ContainsAsync(documentId);
        Assert.True(result);

        fixture.FileSystem.Received(1).FileExists(expectedFilePath);
        fixture.FileSystem.Received(1).GetLastWriteTimeUtc(expectedFilePath);
    }

    [Fact]
    public async Task GetAsync_WhenFileDoesNotExist_ReturnsNull()
    {
        var documentId = TestFixture.GetRandomDocumentId();
        var expectedFilePath = TestFixture.GetExpectedFilePath(documentId);

        fixture.FileSystem.FileExists(expectedFilePath).Returns(false);

        var target = new NPointFileSystemCache();

        var result = await target.GetAsync(documentId);
        Assert.Null(result);

        fixture.FileSystem.Received(1).FileExists(expectedFilePath);
        fixture.FileSystem.DidNotReceive().GetLastWriteTimeUtc(Arg.Any<string>());
        await fixture.FileSystem.DidNotReceive().ReadAllTextAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GetAsync_WhenFileWasCreatedLongerThanTtlAgo_ReturnsNull()
    {
        var documentId = TestFixture.GetRandomDocumentId();
        var expectedFilePath = TestFixture.GetExpectedFilePath(documentId);
        var fileCreationTimeUtc = (DateTimeOffset.UtcNow - TimeSpan.FromSeconds(11)).UtcDateTime;

        fixture.FileSystem.FileExists(expectedFilePath).Returns(true);
        fixture.FileSystem.GetLastWriteTimeUtc(expectedFilePath).Returns(fileCreationTimeUtc);

        var target = new NPointFileSystemCache
        {
            Ttl = TimeSpan.FromSeconds(10)
        };

        var result = await target.GetAsync(documentId);
        Assert.Null(result);

        fixture.FileSystem.Received(1).FileExists(expectedFilePath);
        fixture.FileSystem.Received(1).GetLastWriteTimeUtc(expectedFilePath);
        await fixture.FileSystem.DidNotReceive().ReadAllTextAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GetAsync_WhenFileWasCreatedLessThanTtlAgo_ReturnsFileContents()
    {
        var documentId = TestFixture.GetRandomDocumentId();
        var expectedFilePath = TestFixture.GetExpectedFilePath(documentId);
        var fileCreationTimeUtc = (DateTimeOffset.UtcNow - TimeSpan.FromSeconds(9)).UtcDateTime;
        const string fileContent = """
                                   { "Foo": "bar" }
                                   """;

        fixture.FileSystem.FileExists(expectedFilePath).Returns(true);
        fixture.FileSystem.GetLastWriteTimeUtc(expectedFilePath).Returns(fileCreationTimeUtc);
        fixture.FileSystem.ReadAllTextAsync(expectedFilePath).Returns(fileContent);

        var target = new NPointFileSystemCache
        {
            Ttl = TimeSpan.FromSeconds(10)
        };

        var result = await target.GetAsync(documentId);
        Assert.NotNull(result);
        Assert.Equal(fileContent, result);

        fixture.FileSystem.Received(1).FileExists(expectedFilePath);
        fixture.FileSystem.Received(1).GetLastWriteTimeUtc(expectedFilePath);
        await fixture.FileSystem.Received(1).ReadAllTextAsync(expectedFilePath);
    }

    [Fact]
    public async Task SetAsync_WritesAllTextToFile()
    {
        var documentId = TestFixture.GetRandomDocumentId();
        var expectedFilePath = TestFixture.GetExpectedFilePath(documentId);
        const string setContent = """
                                  { "Foo": "bar" }
                                  """;

        var target = new NPointFileSystemCache();

        await target.SetAsync(documentId, setContent);

        await fixture.FileSystem.Received(1).WriteAllTextAsync(expectedFilePath, setContent);
    }

    [Fact]
    public async Task ClearAsync_DeletesAllCacheFiles()
    {
        var cacheFiles = new List<string>
        {
            TestFixture.GetExpectedFilePath(TestFixture.GetRandomDocumentId()),
            TestFixture.GetExpectedFilePath(TestFixture.GetRandomDocumentId()),
            TestFixture.GetExpectedFilePath(TestFixture.GetRandomDocumentId())
        };

        fixture.FileSystem.GetDirectoryFiles(TestFixture.CachePath).Returns(cacheFiles);

        var target = new NPointFileSystemCache();

        await target.ClearAsync();

        fixture.FileSystem.Received(1).GetDirectoryFiles(TestFixture.CachePath);
        fixture.FileSystem.Received(3).DeleteFile(Arg.Any<string>());
        cacheFiles.ForEach(fixture.FileSystem.Received(1).DeleteFile);
    }
}