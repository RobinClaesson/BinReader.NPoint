using Fortitude.Client;
using Microsoft.Extensions.DependencyInjection;
using BinReader.NPoint.Caching;
using BinReader.NPoint.Config;
using NSubstitute;

namespace BinReader.NPoint.Tests;

public class TestFixture
{
    internal IFileSystem FileSystem { get; }
    public FortitudeClient Fortitude { get; }

    public TestFixture()
    {
        Fortitude = FortitudeClient.Create();

        FileSystem = Substitute.For<IFileSystem>();
        FileSystem.CachePath.Returns(CachePath);
        NPointFileSystemCache.FileSystem = FileSystem;
    }

    public INPointClient CreateNPointClient(Action<NPointConfig>? configuration = null)
    {
        var services = new ServiceCollection();
        services.AddHttpClient();
        services.AddFortitudeClient(Fortitude);

        if (configuration is null)
            services.AddNPoint();
        else
            services.AddNPoint(configuration);

        return services.BuildServiceProvider().GetRequiredService<INPointClient>();
    }

    public FortitudeHandler SetupNPointHttpCall(string? json) => Fortitude
        .Accepts()
        .Get()
        .HttpRoute("/{id}")
        .Returns((_, res) => res.Ok(json));

    public static string GetRandomDocumentId() => Guid.NewGuid().ToString("N")[..20];
    public static string GetExpectedFilePath(string documentId) => $"{CachePath}/{documentId}.json";
    public const string CachePath = "CachePath";
}