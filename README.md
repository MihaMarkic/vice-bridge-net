# VICE Binary Monitor Bridge for .NET

A cross platform .NET 6.0/7.0 library that implements a bridge for communication with [VICE](https://vice-emu.sourceforge.io/) [binary monitor](https://vice-emu.sourceforge.io/vice_13.html#SEC281).

[![NuGet](https://img.shields.io/nuget/v/Righthand.Vice.Bridge.svg)](https://www.nuget.org/packages/Righthand.Vice.Bridge)

| Main   | Develop |
| ------ | ------- |
| [![Build status](https://ci.appveyor.com/api/projects/status/j4mug5wqaqh4ystn/branch/main?svg=true)](https://ci.appveyor.com/project/MihaMarkic/vice-bridge-net/branch/main) | [![Build status](https://ci.appveyor.com/api/projects/status/j4mug5wqaqh4ystn/branch/develop?svg=true)](https://ci.appveyor.com/project/MihaMarkic/vice-bridge-net/branch/develop) |


Tested(limited on Windows 10) and built against VICE 3.7.

**Important**: At the moment, it works only against API v1, not API v2 which is implemented at least in VICE 3.7.

## Quick start

Start VICE with *-binarymonitor* argument so it listens to default port 6502.

First step is to register types with .NET's standard `Microsoft.Extensions.DependencyInjection.IServiceCollection` by calling extension method `Righthand.ViceMonitor.Bridge.AddEngineServices`, like:

```csharp
var collection = new ServiceCollection();
collection.AddEngineServices();
```

After IoC is setup, `IViceBridge` has to be resolved through IoC. Optionally `ConnectionChanged` even handler can be used to track connection status (also available through `IViceBridge.IsConnected` property) and `ViceResponse` event handler can be used to receive unbound responses from VICE.

```csharp
bridge.ConnectedChanged += Bridge_ConnectedChanged;
bridge.Start();
```

After `IViceBridge.IsConnected` property becomes `true`, command can be sent to VICE and responses will flow back. Here is a ping command:

```csharp
var ping = bridge.EnqueueCommand(new PingCommand());
var commandResponse = await ping.Response;
if (commandResponse.IsSuccess)
{
	// response is of type PingCommand expects
	var response = cr.Response;
	...
}
else
{
	Console.WriteLine($"An error with code {commandResponse.ErrorCode} occurred");
}
```

## A bit more about internal working

Library is using .NET's `Microsoft.Extensions.DependencyInjection` IoC system and is mandatory to initialize it before starting `ViceBridge`.

Client typically creates immutable commands and enqueues them into bridge. ViceBridge will process them in FIFO manner. Once commands are sent to VICE, bridge matches response (or in some case responses) to issued command. When client wants to read VICE's response or just waits for it, it can await `ViceCommand.Response` task.

Commands are using `Righthand.ViceMonitor.Bridge.Commands` namespace and are modelled to match VICE's binary protocol with few enhancements where available. For example, there is no need for `FileNameIndex` as it can be read from `FileName` string property.

Internally byte arrays for input and output are retrieved from `ArrayPool<byte>.Shared` through custom `BufferManager` class which packs the arrays into `ManagedBuffer` class. Some properties exposes said `ManagedBuffer`. Read Memory Management chapter below to avoid memory leaks.

The responses that are not bound to any command are accessible through `IViceBridge.ViceResponse` event. Responses are using `Righthand.ViceMonitor.Bridge.Responses` namespace.

## Memory management

Caller is required to dispose responses that implement `IDisposable` after all data has been processed. Usually those responses have at least one property of `ManagedBuffer` type which is borrowing byte array from a shared pool. Failing to call `Dispose()` on response will result in memory leak.

At the moment of this writing there are only two such responses: `MemoryGetResponse` and `DisplayGetResponse`.

When a `ManagedBuffer` instance is passed to a command then `ViceBridge` will dispose the command and consequently the given `ManagedBuffer`. Once command in enqueued the caller shouldn't modify passed `ManagedBuffer` anymore as there are no guarantees when it is being disposed.

At the moment of this writing there is only one such command: `MemorySetCommand`.

## Playground sample

Playground sample is a console application that is used for testing and for sample purposes. It demonstrates the basics and some simple tasks.