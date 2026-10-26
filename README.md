# BinReader.NPoint

[![tests](https://github.com/RobinClaesson/BinReader.NPoint/actions/workflows/tests.yml/badge.svg)](https://github.com/RobinClaesson/BinReader.NPoint/actions/workflows/tests.yml)

A .NET client for reading and caching JSON bins from [npoint](https://www.npoint.io)
by [azirbel](https://github.com/azirbel/npoint).

## Usage

Install the package:

```sh
dotnet add package BinReader.NPoint
```

Register the NPoint services and IHttpClientFactory:

```csharp
using BinReader.NPoint;

services.AddNPoint();
services.AddHttpClient(); //Or any other IHttpClientFactory
```

Pass the bin ID to retrieve JSON as text or deserialize it into a type:

```csharp
public class Example(INPointClient client)
{
    public async Task Run()
    {
        var json = await client.GetAsync("97d71acb1b5b8164097b");
        var data = await client.GetAsync<ExampleData>("97d71acb1b5b8164097b");

        Console.WriteLine(json);
        Console.WriteLine(data);
    }

    public record ExampleData(string Name, double Version, string Info);
}
```

Example output:

```
{"Info":"Example object for the README of https://github.com/RobinClaesson/BinReader.NPoint, a .NET client for reading and caching n:point bins","Name":"BinReader.NPoint Example","Version":1}

ExampleData { Name = BinReader.NPoint Example, Version = 1, Info = Example object for the README of https://github.com/RobinClaesson/BinReader.NPoint, a .NET client for reading and caching n:point bins }
```

Both methods accept an optional cancellation token.

### Configurations

By default, the client uses `NPointFileSystemCache` with a five-minute time to live (TTL), default `System.Text.Json`
serializer options, and a scoped service lifetime.

You can customize these settings when registering services:

```csharp
using System.Text.Json;
using BinReader.NPoint.Caching;

services.AddNPoint(config =>
{
    config.UseCache(new NPointFileSystemCache
    {
        Ttl = TimeSpan.FromMinutes(10)
    });
    
    config.JsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    
    config.ServiceLifetime = ServiceLifetime.Transient;
});
```

`UseCache` accepts any `INPointCache` implementation. To disable caching and fetch from npoint on every call, register
with:

```csharp
services.AddNPoint(config => config.NoCache());
```

## Credits & Dependencies

- n:point - A lightweight data store for your app or prototype, and the target for this project.
    - [Website](https://www.npoint.io/)
    - [Source](https://github.com/azirbel/npoint)
- Fortitude - Mocking HTTP API responses for testing.
    - [Source](https://github.com/Timmoth/Fortitude)
- NSubstitute - Mocking classes and interfaces for testing.
    - [Website](https://nsubstitute.github.io/)
    - [Source](https://github.com/nsubstitute/NSubstitute)
- xUnit v3 - Testing framework.
    - [Website](https://xunit.net/index.html?tabs=cs)
    - [Source](https://github.com/xunit/xunit)

# License & Copyright

BinReader.NPoint released under the [MIT license](LICENSE). Copyright (c) 2026 Robin Claesson.

n:point and the n:point logo are the property of Alexander
Zirbel. [Copyright (c) 2017-2018](https://github.com/azirbel/npoint#copyright--license)
